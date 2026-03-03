#Requires -Version 5.1
<#
.SYNOPSIS
    Avvia l'intero stack di sviluppo ILP eFTI Connector.

.DESCRIPTION
    1. Verifica che Docker Desktop sia in esecuzione
    2. Avvia i container infrastrutturali via docker-compose
    3. Attende che MariaDB, RabbitMQ e Keycloak siano healthy
    4. Avvia tutti i microservizi .NET in finestre PowerShell separate

.PARAMETER SkipDocker
    Salta l'avvio di docker-compose (se i container sono già attivi).

.PARAMETER SkipInfra
    Avvia solo i servizi .NET (assume che docker-compose sia già up).

.PARAMETER Services
    Sottoinsieme di servizi da avviare. Default: tutti.
    Valori: FormInput, QueryProxy, Validation, Normalization,
            EftiGateway, ResponseHandler, Notification, Retry

.EXAMPLE
    .\start-dev.ps1
    .\start-dev.ps1 -SkipDocker
    .\start-dev.ps1 -Services FormInput,QueryProxy,Validation,Normalization
#>
param(
    [switch]$SkipDocker,
    [switch]$SkipInfra,
    [switch]$SkipFrontendBuild,
    [string[]]$Services = @(
        'FormInput','QueryProxy',
        'Validation','Normalization',
        'EftiGateway','ResponseHandler',
        'Notification','Retry'
    )
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$Root        = $PSScriptRoot
$ComposeFile = Join-Path $Root "infra\docker\docker-compose.yml"
$EnvFile     = Join-Path $Root "infra\docker\.env"
$ServicesDir = Join-Path $Root "src\Services"

# ── Mappa servizio → (cartella progetto, profilo launch) ─────────────────────
$ServiceMap = [ordered]@{
    FormInput       = @{ Dir = 'ilp_efti_connector.FormInputService';       Profile = 'http' }
    QueryProxy      = @{ Dir = 'ilp_efti_connector.QueryProxyService';      Profile = 'http' }
    Validation      = @{ Dir = 'ilp_efti_connector.ValidationService';      Profile = 'ilp_efti_connectorValidationService' }
    Normalization   = @{ Dir = 'ilp_efti_connector.NormalizationService';   Profile = 'ilp_efti_connectorNormalizationService' }
    EftiGateway     = @{ Dir = 'ilp_efti_connector.EftiGatewayService';     Profile = 'ilp_efti_connectorEftiGatewayService' }
    ResponseHandler = @{ Dir = 'ilp_efti_connector.ResponseHandlerService'; Profile = 'ilp_efti_connectorResponseHandlerService' }
    Notification    = @{ Dir = 'ilp_efti_connector.NotificationService';    Profile = 'ilp_efti_connectorNotificationService' }
    Retry           = @{ Dir = 'ilp_efti_connector.RetryService';           Profile = 'ilp_efti_connectorRetryService' }
}

function Write-Step([string]$msg) {
    Write-Host "`n── $msg" -ForegroundColor Cyan
}
function Write-Ok([string]$msg) {
    Write-Host "   ✔ $msg" -ForegroundColor Green
}
function Write-Warn([string]$msg) {
    Write-Host "   ⚠ $msg" -ForegroundColor Yellow
}

# ── 1. Verifica Docker ───────────────────────────────────────────────────────
if (-not $SkipDocker -and -not $SkipInfra) {
    Write-Step "Verifica Docker Desktop"
    try {
        $null = docker info 2>&1
        Write-Ok "Docker in esecuzione"
    } catch {
        Write-Error "Docker Desktop non è in esecuzione. Avviarlo e riprovare."
        exit 1
    }
}

# ── 2. docker-compose up ─────────────────────────────────────────────────────
if (-not $SkipDocker -and -not $SkipInfra) {
    Write-Step "Avvio container infrastrutturali"
    docker compose -f $ComposeFile --env-file $EnvFile up -d 2>&1 | Out-Null
    Write-Ok "docker-compose eseguito"
}

# ── 3. Build e riavvio frontend React ───────────────────────────────────────────
if (-not $SkipFrontendBuild) {
    Write-Step "Build frontend React"
    Write-Host "   Compilazione in corso (npm run build dentro Docker)…" -ForegroundColor DarkGray

    $buildOutput = docker compose -f $ComposeFile --env-file $EnvFile build frontend --no-cache 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Warn "Build frontend fallita — output:"
        $buildOutput | ForEach-Object { Write-Host "   $_" -ForegroundColor DarkRed }
    } else {
        Write-Ok "Image frontend costruita"
        docker compose -f $ComposeFile --env-file $EnvFile up -d --force-recreate frontend 2>&1 | Out-Null
        Write-Ok "Container frontend riavviato (http://localhost:3100)"
    }
} else {
    Write-Step "Build frontend"
    Write-Warn "Build saltata (-SkipFrontendBuild)"
}

# ── 4. Attesa dipendenze ───────────────────────────────────────────
if (-not $SkipInfra) {
    Write-Step "Attesa MariaDB"
    $maxWait = 60; $waited = 0
    do {
        Start-Sleep -Seconds 3; $waited += 3
        $health = docker inspect --format='{{.State.Health.Status}}' ilp-efti-connector-mariadb 2>&1
    } while ($health -ne 'healthy' -and $waited -lt $maxWait)
    if ($health -ne 'healthy') { Write-Warn "MariaDB non healthy dopo ${maxWait}s — continuo comunque" }
    else { Write-Ok "MariaDB healthy" }

    Write-Step "Attesa RabbitMQ"
    $waited = 0
    do {
        Start-Sleep -Seconds 3; $waited += 3
        $health = docker inspect --format='{{.State.Health.Status}}' ilp-efti-connector-rabbitmq 2>&1
    } while ($health -ne 'healthy' -and $waited -lt $maxWait)
    if ($health -ne 'healthy') { Write-Warn "RabbitMQ non healthy dopo ${maxWait}s — continuo comunque" }
    else { Write-Ok "RabbitMQ healthy" }

    Write-Step "Attesa Keycloak (max 90s)"
    $waited = 0
    do {
        Start-Sleep -Seconds 5; $waited += 5
        try {
            $resp = Invoke-WebRequest -Uri "http://localhost:8080/realms/master" -UseBasicParsing -TimeoutSec 3 -ErrorAction SilentlyContinue
            $kc = $resp.StatusCode -eq 200
        } catch { $kc = $false }
    } while (-not $kc -and $waited -lt 90)
    if (-not $kc) { Write-Warn "Keycloak non pronto dopo 90s — continuo comunque" }
    else { Write-Ok "Keycloak pronto" }
}

# ── 5. Ferma eventuali istanze dotnet già attive ─────────────────────────────
Write-Step "Stop istanze dotnet precedenti"
$prev = Get-Process -Name dotnet -ErrorAction SilentlyContinue
if ($prev) {
    $prev | Stop-Process -Force
    Start-Sleep -Seconds 2
    Write-Ok "Processi precedenti terminati ($($prev.Count))"
} else {
    Write-Ok "Nessun processo dotnet attivo"
}

# ── 6. Avvio servizi .NET ────────────────────────────────────────────────────
Write-Step "Avvio microservizi .NET"

$started = @()
foreach ($name in $Services) {
    if (-not $ServiceMap.Contains($name)) {
        Write-Warn "Servizio sconosciuto: $name — ignorato"
        continue
    }
    $svc     = $ServiceMap[$name]
    $projDir = Join-Path $ServicesDir $svc.Dir
    $profile = $svc.Profile

    if (-not (Test-Path $projDir)) {
        Write-Warn "$name — cartella non trovata: $projDir"
        continue
    }

    Start-Process powershell -ArgumentList (
        "-NoExit",
        "-Command",
        "cd '$projDir'; `$host.UI.RawUI.WindowTitle = '$name'; dotnet run --launch-profile '$profile'"
    )

    $started += $name
    Write-Ok "$name avviato (profilo: $profile)"
}

# ── 7. Riepilogo ───────────────────────────────────────────
Write-Host "`n══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " Stack di sviluppo avviato" -ForegroundColor Cyan
Write-Host "══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " Infrastruttura (Docker):"
Write-Host "   MariaDB   → localhost:3306"
Write-Host "   RabbitMQ  → localhost:5672  (UI: http://localhost:15672)"
Write-Host "   Keycloak  → http://localhost:8080"
Write-Host "   Frontend  → http://localhost:3100"
Write-Host ""
Write-Host " Microservizi .NET avviati:" -ForegroundColor Green
foreach ($s in $started) { Write-Host "   • $s" }
Write-Host ""
Write-Host " Endpoint HTTP:"
Write-Host "   FormInput API  → http://localhost:5006"
Write-Host "   QueryProxy API → http://localhost:5021"
Write-Host "══════════════════════════════════════════`n" -ForegroundColor Cyan

# Esempi di avvio
# .\start-dev.ps1                                    # avvio completo (docker + build frontend + tutti i servizi)
# .\start-dev.ps1 -SkipDocker                        # solo build frontend + .NET (docker già up)
# .\start-dev.ps1 -SkipFrontendBuild                 # salta la build del frontend
# .\start-dev.ps1 -SkipDocker -SkipFrontendBuild     # solo servizi .NET
# .\start-dev.ps1 -Services FormInput,QueryProxy,Validation,Normalization

#Requires -Version 5.1
<#
.SYNOPSIS
    Ferma l'intero stack di sviluppo ILP eFTI Connector.

.DESCRIPTION
    1. Termina tutti i processi dotnet avviati dallo start-dev
    2. Ferma i container Docker (opzionale)

.PARAMETER KeepDocker
    Non fermare i container Docker (utile per riavviare solo i servizi .NET).

.PARAMETER KeepData
    Usato con -KeepDocker=false: esegue 'docker compose stop' invece di 'down'
    preservando i volumi (dati MariaDB, RabbitMQ, ecc.).

.EXAMPLE
    .\stop-dev.ps1                   # ferma .NET + docker stop (preserva volumi)
    .\stop-dev.ps1 -KeepDocker       # ferma solo i processi .NET
    .\stop-dev.ps1 -KeepData:$false  # ferma .NET + docker down (rimuove container)
#>
param(
    [switch]$KeepDocker,
    [bool]$KeepData = $true
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$Root        = $PSScriptRoot
$ComposeFile = Join-Path $Root "infra\docker\docker-compose.yml"
$EnvFile     = Join-Path $Root "infra\docker\.env"

function Write-Step([string]$msg) { Write-Host "`n── $msg" -ForegroundColor Cyan }
function Write-Ok([string]$msg)   { Write-Host "   ✔ $msg" -ForegroundColor Green }
function Write-Warn([string]$msg) { Write-Host "   ⚠ $msg" -ForegroundColor Yellow }

# ── 1. Ferma processi dotnet ─────────────────────────────────────────────────
Write-Step "Stop microservizi .NET"
$procs = Get-Process -Name dotnet -ErrorAction SilentlyContinue
if ($procs) {
    $procs | Stop-Process -Force
    Start-Sleep -Seconds 2
    Write-Ok "Terminati $($procs.Count) processo/i dotnet"
} else {
    Write-Ok "Nessun processo dotnet attivo"
}

# ── 2. Ferma container Docker ────────────────────────────────────────────────
if (-not $KeepDocker) {
    Write-Step "Stop container Docker"
    try {
        $null = docker info 2>&1
    } catch {
        Write-Warn "Docker Desktop non raggiungibile — skip"
        $KeepDocker = $true
    }
}

if (-not $KeepDocker) {
    if ($KeepData) {
        docker compose -f $ComposeFile --env-file $EnvFile stop 2>&1 | Out-Null
        Write-Ok "Container fermati (volumi preservati)"
    } else {
        docker compose -f $ComposeFile --env-file $EnvFile down 2>&1 | Out-Null
        Write-Ok "Container e network rimossi (volumi preservati)"
    }
}

# ── Riepilogo ────────────────────────────────────────────────────────────────
Write-Host "`n══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " Stack di sviluppo fermato" -ForegroundColor Cyan
Write-Host "══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " Microservizi .NET → STOP"
if ($KeepDocker) {
    Write-Host " Container Docker  → invariati" -ForegroundColor Yellow
} elseif ($KeepData) {
    Write-Host " Container Docker  → STOP (dati preservati)"
} else {
    Write-Host " Container Docker  → RIMOSSI (dati preservati nei volumi)"
}
Write-Host "══════════════════════════════════════════`n" -ForegroundColor Cyan

# Esempi di utilizzo
# .\stop-dev.ps1                    # ferma .NET + docker stop (preserva volumi)
# .\stop-dev.ps1 -KeepDocker        # ferma solo i processi .NET
# .\stop-dev.ps1 -KeepData:$false   # ferma .NET + docker down (rimuove container)

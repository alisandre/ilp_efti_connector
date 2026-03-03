# EFTI Connector Platform — Infrastructure Guide

> **Versione:** 1.1 · **Data:** Febbraio 2026  
> Guida operativa per il provisioning dell'infrastruttura locale (dev) e di produzione.
> Repository: [github.com/alisandre/ilp_efti_connector](https://github.com/alisandre/ilp_efti_connector) · Branch: `main`

---

## Indice

1. [Prerequisiti](#1-prerequisiti)
2. [Struttura della Solution](#2-struttura-della-solution)
3. [Ambiente di Sviluppo Locale (Docker Compose)](#3-ambiente-di-sviluppo-locale-docker-compose)
4. [Configurazione dei Servizi](#4-configurazione-dei-servizi)
5. [Database — MariaDB](#5-database--mariadb)
6. [Message Broker — RabbitMQ](#6-message-broker--rabbitmq)
7. [Cache — Redis](#7-cache--redis)
8. [Identity Provider — Keycloak](#8-identity-provider--keycloak)
9. [API Gateway — YARP](#9-api-gateway--yarp)
10. [Logging — Serilog + Seq](#10-logging--serilog--seq)
11. [Monitoring — Prometheus + Grafana](#11-monitoring--prometheus--grafana)
12. [Gateway EFTI — Fase 1 (MILOS) e Fase 2 (EFTI Native)](#12-gateway-efti--fase-1-milos-e-fase-2-efti-native)
13. [CI/CD Pipeline](#13-cicd-pipeline)
14. [Produzione — Kubernetes](#14-produzione--kubernetes)
15. [Secrets Management](#15-secrets-management)
16. [Health Checks e Readiness](#16-health-checks-e-readiness)
17. [Runbook Operativo](#17-runbook-operativo)

---

## 1. Prerequisiti

### Strumenti richiesti

| Strumento | Versione minima | Installazione |
|---|---|---|
| .NET SDK | 9.0 | https://dot.net |
| Docker Desktop | 4.x | https://docker.com |
| Docker Compose | 2.x | incluso in Docker Desktop |
| Node.js | 22.x LTS | https://nodejs.org (per frontend React) |
| kubectl | 1.29+ | https://kubernetes.io/docs/tasks/tools |
| Helm | 3.14+ | https://helm.sh |
| dotnet-ef (EF Core CLI) | 9.x | `dotnet tool install -g dotnet-ef` |

### Verifica installazione

```bash
dotnet --version          # 9.0.x
docker --version          # 27.x
docker compose version    # 2.x
kubectl version --client  # 1.29+
helm version              # 3.14+
dotnet ef --version       # 9.0.x
```

---

## 2. Struttura della Solution

```
ilp_efti_connector/                         ← root della repository
│                                             https://github.com/alisandre/ilp_efti_connector
├── INFRASTRUCTURE.md                       ← questo file
├── ilp_efti_connector.sln
│
├── src/
│   ├── Core/
│   │   ├── ilp-efti-connector.Domain/
│   │   ├── ilp-efti-connector.Application/
│   │   └── ilp-efti-connector.Infrastructure/
│   │
│   ├── Gateway/
│   │   ├── ilp-efti-connector.Gateway.Contracts/
│   │   ├── ilp-efti-connector.Gateway.Milos/        ← Fase 1
│   │   └── ilp-efti-connector.Gateway.EftiNative/   ← Fase 2
│   │
│   ├── Services/
│   │   ├── ilp-efti-connectorApiGateway/
│   │   ├── ilp-efti-connectorValidationService/
│   │   ├── ilp-efti-connectorNormalizationService/
│   │   ├── ilp-efti-connectorEftiGatewayService/
│   │   ├── ilp-efti-connectorResponseHandlerService/
│   │   ├── ilp-efti-connectorNotificationService/
│   │   ├── ilp-efti-connectorQueryProxyService/
│   │   ├── ilp-efti-connectorRetryService/
│   │   └── ilp-efti-connectorFormInputService/
│   │
│   └── Shared/
│       ├── ilp-efti-connector.Shared.Contracts/
│       └── ilp-efti-connector.Shared.Infrastructure/
│
├── tests/
│   ├── ilp-efti-connector.Domain.Tests/
│   ├── ilp-efti-connector.Application.Tests/
│   ├── ilp-efti-connector.Gateway.Milos.Tests/
│   ├── ilp-efti-connector.Gateway.EftiNative.Tests/
│   └── ilp-efti-connectorIntegrationTests/
│
├── frontend/                               ← React SPA
│   └── efti-connector-ui/
│
├── infra/                                  ← tutto ciò che riguarda infrastruttura
│   ├── docker/
│   │   ├── docker-compose.yml              ← stack completo locale
│   │   ├── docker-compose.override.yml     ← override per sviluppo
│   │   ├── docker-compose.test.yml         ← stack per integration test
│   │   └── dockerfiles/                    ← Dockerfile per ogni microservizio
│   │       ├── Dockerfile.apigateway
│   │       ├── Dockerfile.validationservice
│   │       └── ...
│   │
│   ├── keycloak/
│   │   ├── realm-efti.json                 ← export realm Keycloak
│   │   └── themes/                         ← tema custom (opzionale)
│   │
│   ├── grafana/
│   │   ├── provisioning/
│   │   │   ├── datasources/prometheus.yml
│   │   │   └── dashboards/efti-overview.json
│   │   └── dashboards/
│   │
│   ├── prometheus/
│   │   └── prometheus.yml
│   │
│   ├── mariadb/
│   │   └── init/
│   │       └── 01-create-databases.sql
│   │
│   └── k8s/                                ← Kubernetes / Helm
│       ├── helm/
│       │   └── efti-connector/
│       │       ├── Chart.yaml
│       │       ├── values.yaml
│       │       ├── values.staging.yaml
│       │       ├── values.production.yaml
│       │       └── templates/
│       └── namespaces.yaml
│
└── scripts/
    ├── dev-up.sh                           ← avvio ambiente locale
    ├── dev-down.sh
    ├── db-migrate.sh                       ← esegue EF Core migrations
    ├── keycloak-import.sh                  ← importa realm
    └── smoke-test.sh                       ← test rapido post-deploy
```

---

## 3. Ambiente di Sviluppo Locale (Docker Compose)

### Avvio rapido — Linux / macOS (Bash)

```bash
# 1. Clona il repository
git clone https://github.com/alisandre/ilp_efti_connector.git
cd ilp_efti_connector

# 2. Copia le variabili d'ambiente
cp infra/docker/.env.example infra/docker/.env
# ⚠️  Edita .env con le tue configurazioni locali prima di procedere

# 3. Avvia lo stack completo
./scripts/dev-up.sh

# oppure manualmente
cd infra/docker
docker compose up -d

# 4. Esegui le migrations del database
./scripts/db-migrate.sh

# 5. Importa il realm Keycloak
./scripts/keycloak-import.sh
```

### Avvio rapido — Windows (PowerShell)

> **Prerequisiti:** Docker Desktop installato e in esecuzione, Git, .NET SDK 9.0, `dotnet-ef` (`dotnet tool install -g dotnet-ef`)

```powershell
# 1. Clona il repository
git clone https://github.com/alisandre/ilp_efti_connector.git
cd ilp_efti_connector

# 2. Copia le variabili d'ambiente
copy infra\docker\.env.example infra\docker\.env
# ⚠️  Apri infra\docker\.env con un editor e modifica le password prima di procedere

# 3. Avvia lo stack completo
cd infra\docker
docker compose --profile dev up -d --build
cd ..\..

# Servizi disponibili dopo l'avvio:
#   MariaDB      → localhost:3306
#   RabbitMQ     → http://localhost:15672  (efti_rabbit / changeme_rabbit)
#   Redis        → localhost:6379
#   Keycloak     → http://localhost:8080   (admin / changeme_kc)
#   Seq (logs)   → http://localhost:8888
#   Grafana      → http://localhost:3001   (admin / admin)
#   Prometheus   → http://localhost:9090
#   MILOS Mock   → http://localhost:9999

# 4. Esegui le migrations del database
#    Attendi che MariaDB sia healthy (~20s), poi:
dotnet ef database update `
  --project src\Shared\ilp_efti_connector.Shared.Infrastructure `
  --startup-project src\Services\ilp_efti_connector.ApiGateway `
  --connection "Server=localhost;Port=3306;Database=efti_connector;User=efti_user;Password=MariaDB@2026!;"

# 5. Importa il realm Keycloak
#    Il realm viene importato automaticamente all'avvio del container da infra\keycloak\realm-efti.json
#    Admin console: http://localhost:8080  (admin / changeme_kc)
#
#    Se occorre reimportare manualmente:
docker exec ilp-efti-connector-keycloak /opt/keycloak/bin/kc.sh import --file /opt/keycloak/data/import/realm-efti.json
```

#### Comandi utili su Windows

```powershell
# Stato dei container
docker compose -f infra\docker\docker-compose.yml ps

# Log in tempo reale di un servizio
docker logs -f efti-rabbitmq

# Fermare lo stack
cd infra\docker
docker compose down

# Fermare lo stack e rimuovere i volumi (reset completo)
docker compose down -v
```

#### Troubleshooting Windows

| Errore | Causa | Soluzione |
|---|---|---|
| `open //./pipe/dockerDesktopLinuxEngine: The system cannot find the file specified` | Docker Desktop non è avviato o è in modalità **Windows containers** | 1. Avvia Docker Desktop e attendi l'icona stabile nella system tray. 2. Tasto destro sull'icona → **Switch to Linux containers** |
| `the attribute 'version' is obsolete` | Compose v2 non usa più il campo `version` | Warning innocuo; il campo è già stato rimosso da `docker-compose.yml` |
| `port is already allocated` | Una porta (es. 3306, 5672) è già occupata da un altro processo | `netstat -ano \| findstr :<porta>` per trovare il PID, poi terminarlo da Task Manager |

### File `infra/docker/.env.example`

```dotenv
# ── MariaDB ─────────────────────────────────────────────────────
MARIADB_ROOT_PASSWORD=changeme_root
MARIADB_DATABASE=efti_connector
MARIADB_USER=efti_user
MARIADB_PASSWORD=changeme_efti

# ── RabbitMQ ────────────────────────────────────────────────────
RABBITMQ_DEFAULT_USER=efti_rabbit
RABBITMQ_DEFAULT_PASS=changeme_rabbit
RABBITMQ_DEFAULT_VHOST=efti

# ── Redis ───────────────────────────────────────────────────────
REDIS_PASSWORD=changeme_redis

# ── Keycloak ────────────────────────────────────────────────────
KEYCLOAK_ADMIN=admin
KEYCLOAK_ADMIN_PASSWORD=changeme_kc
KEYCLOAK_DB_PASSWORD=changeme_kc_db

# ── Seq ─────────────────────────────────────────────────────────
SEQ_API_KEY=changeme_seq

# ── Gateway EFTI (Fase 1 — MILOS) ───────────────────────────────
EFTI_GATEWAY_PROVIDER=Milos
MILOS_BASE_URL=https://sandbox.milos-tfp.example.com/api/ecmr-service/
MILOS_API_KEY=your_milos_sandbox_api_key

# ── Gateway EFTI (Fase 2 — Native, commentato finché non serve) ──
# EFTI_GATEWAY_PROVIDER=EftiNative
# EFTI_NATIVE_BASE_URL=https://efti-gate.example.eu/
# EFTI_NATIVE_CLIENT_ID=efti-connector-client
```

### File `infra/docker/docker-compose.yml`

```yaml
version: "3.9"

networks:
  efti-net:
    driver: bridge

volumes:
  mariadb-data:
  rabbitmq-data:
  redis-data:
  seq-data:
  grafana-data:
  prometheus-data:

services:

  # ── Database ──────────────────────────────────────────────────
  mariadb:
    image: mariadb:11.4
    container_name: efti-mariadb
    restart: unless-stopped
    environment:
      MARIADB_ROOT_PASSWORD: ${MARIADB_ROOT_PASSWORD}
      MARIADB_DATABASE: ${MARIADB_DATABASE}
      MARIADB_USER: ${MARIADB_USER}
      MARIADB_PASSWORD: ${MARIADB_PASSWORD}
    ports:
      - "3306:3306"
    volumes:
      - mariadb-data:/var/lib/mysql
      - ./mariadb/init:/docker-entrypoint-initdb.d
    networks:
      - efti-net
    healthcheck:
      test: ["CMD", "healthcheck.sh", "--connect", "--innodb_initialized"]
      interval: 10s
      timeout: 5s
      retries: 5

  # ── Message Broker ────────────────────────────────────────────
  rabbitmq:
    image: rabbitmq:3.13-management
    container_name: efti-rabbitmq
    restart: unless-stopped
    environment:
      RABBITMQ_DEFAULT_USER: ${RABBITMQ_DEFAULT_USER}
      RABBITMQ_DEFAULT_PASS: ${RABBITMQ_DEFAULT_PASS}
      RABBITMQ_DEFAULT_VHOST: ${RABBITMQ_DEFAULT_VHOST}
    ports:
      - "5672:5672"
      - "15672:15672"    # Management UI
    volumes:
      - rabbitmq-data:/var/lib/rabbitmq
    networks:
      - efti-net
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "ping"]
      interval: 15s
      timeout: 10s
      retries: 5

  # ── Cache ─────────────────────────────────────────────────────
  redis:
    image: redis:7-alpine
    container_name: efti-redis
    restart: unless-stopped
    command: redis-server --requirepass ${REDIS_PASSWORD}
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    networks:
      - efti-net
    healthcheck:
      test: ["CMD", "redis-cli", "-a", "${REDIS_PASSWORD}", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

  # ── Identity Provider ─────────────────────────────────────────
  keycloak:
    image: quay.io/keycloak/keycloak:24.0
    container_name: efti-keycloak
    restart: unless-stopped
    command: start-dev --import-realm
    environment:
      KEYCLOAK_ADMIN: ${KEYCLOAK_ADMIN}
      KEYCLOAK_ADMIN_PASSWORD: ${KEYCLOAK_ADMIN_PASSWORD}
      KC_DB: postgres
      KC_DB_URL: jdbc:postgresql://keycloak-db:5432/keycloak
      KC_DB_USERNAME: keycloak
      KC_DB_PASSWORD: ${KEYCLOAK_DB_PASSWORD}
      KC_HOSTNAME_STRICT: false
      KC_HTTP_ENABLED: true
    ports:
      - "8080:8080"
    volumes:
      - ../keycloak:/opt/keycloak/data/import
    depends_on:
      keycloak-db:
        condition: service_healthy
    networks:
      - efti-net

  keycloak-db:
    image: postgres:16-alpine
    container_name: efti-keycloak-db
    restart: unless-stopped
    environment:
      POSTGRES_DB: keycloak
      POSTGRES_USER: keycloak
      POSTGRES_PASSWORD: ${KEYCLOAK_DB_PASSWORD}
    volumes:
      - keycloak-db-data:/var/lib/postgresql/data
    networks:
      - efti-net
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U keycloak"]
      interval: 10s
      timeout: 5s
      retries: 5

  # ── Logging ───────────────────────────────────────────────────
  seq:
    image: datalust/seq:2024
    container_name: efti-seq
    restart: unless-stopped
    environment:
      ACCEPT_EULA: Y
      SEQ_FIRSTRUN_ADMINPASSWORDHASH: ""
    ports:
      - "5341:5341"    # Ingestion API
      - "8888:80"      # Web UI → http://localhost:8888
    volumes:
      - seq-data:/data
    networks:
      - efti-net

  # ── Monitoring ────────────────────────────────────────────────
  prometheus:
    image: prom/prometheus:v2.51.0
    container_name: efti-prometheus
    restart: unless-stopped
    ports:
      - "9090:9090"
    volumes:
      - ../prometheus/prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus-data:/prometheus
    networks:
      - efti-net

  grafana:
    image: grafana/grafana:10.4.0
    container_name: efti-grafana
    restart: unless-stopped
    environment:
      GF_SECURITY_ADMIN_PASSWORD: admin
      GF_USERS_ALLOW_SIGN_UP: false
    ports:
      - "3001:3000"    # UI → http://localhost:3001
    volumes:
      - grafana-data:/var/lib/grafana
      - ../grafana/provisioning:/etc/grafana/provisioning
      - ../grafana/dashboards:/var/lib/grafana/dashboards
    depends_on:
      - prometheus
    networks:
      - efti-net

  # ── Mock MILOS (solo dev — Fase 1) ───────────────────────────
  milos-mock:
    image: wiremock/wiremock:3.5.2
    container_name: efti-milos-mock
    ports:
      - "9999:8080"
    volumes:
      - ./milos-mock:/home/wiremock/__files
      - ./milos-mock/mappings:/home/wiremock/mappings
    networks:
      - efti-net
    profiles:
      - dev    # avviato solo con: docker compose --profile dev up

volumes:
  keycloak-db-data:
```

### Script `scripts/dev-up.sh`

```bash
#!/usr/bin/env bash
set -euo pipefail

echo "▶ Avvio stack EFTI Connector..."
cd infra/docker

# Copia .env se non esiste
if [ ! -f .env ]; then
  echo "⚠  File .env non trovato — copio .env.example"
  cp .env.example .env
  echo "✏  Edita infra/docker/.env prima di continuare."
  exit 1
fi

docker compose --profile dev up -d --build

echo ""
echo "✅  Stack avviato. Servizi disponibili:"
echo "   MariaDB      → localhost:3306"
echo "   RabbitMQ     → http://localhost:15672  (guest/guest)"
echo "   Redis        → localhost:6379"
echo "   Keycloak     → http://localhost:8080"
echo "   Seq (logs)   → http://localhost:8888"
echo "   Grafana      → http://localhost:3001   (admin/admin)"
echo "   Prometheus   → http://localhost:9090"
echo "   MILOS Mock   → http://localhost:9999"
echo ""
echo "▶ Esegui le migrations:"
echo "   ./scripts/db-migrate.sh"
```

---

## 4. Configurazione dei Servizi

### `appsettings.json` base (ogni microservizio)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=efti_connector;User=efti_user;Password=changeme_efti;"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "VirtualHost": "efti",
    "Username": "efti_rabbit",
    "Password": "changeme_rabbit"
  },
  "Redis": {
    "ConnectionString": "localhost:6379,password=changeme_redis"
  },
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/efti",
    "Audience": "efti-api"
  },
  "Seq": {
    "ServerUrl": "http://localhost:5341"
  },
  "EftiGateway": {
    "Provider": "Milos",
    "Milos": {
      "BaseUrl": "http://localhost:9999/api/ecmr-service/",
      "ApiKey": "dev-mock-api-key"
    },
    "EftiNative": {
      "BaseUrl": "",
      "ClientId": "",
      "CertificatePath": ""
    }
  }
}
```

### `appsettings.Production.json` (override per produzione)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=mariadb-cluster;Port=3306;Database=efti_connector;User=efti_user;Password=$(DB_PASSWORD);SslMode=Required;"
  },
  "EftiGateway": {
    "Provider": "Milos",
    "Milos": {
      "BaseUrl": "https://tfp.milos.circlegroup.eu/api/ecmr-service/",
      "ApiKey": "$(MILOS_API_KEY)"
    }
  },
  "Serilog": {
    "MinimumLevel": { "Default": "Warning" }
  }
}
```

> Le variabili `$(...)` vengono sostituite dai secrets iniettati da Kubernetes/Vault a runtime.

---

## 5. Database — MariaDB

### Schema iniziale `infra/mariadb/init/01-create-databases.sql`

```sql
-- Creato automaticamente al primo avvio del container MariaDB
CREATE DATABASE IF NOT EXISTS efti_connector CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE IF NOT EXISTS efti_hangfire CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

GRANT ALL PRIVILEGES ON efti_connector.* TO 'efti_user'@'%';
GRANT ALL PRIVILEGES ON efti_hangfire.*  TO 'efti_user'@'%';
FLUSH PRIVILEGES;
```

### EF Core Migrations

```bash
# Aggiunge una nuova migration
dotnet ef migrations add <NomeMigration> \
  --project src/Shared/ilp_efti_connector.Shared.Infrastructure \
  --startup-project src/Services/ilp-efti-connectorApiGateway \
  --output-dir Migrations

# Applica tutte le migrations pendenti
dotnet ef database update \
  --project src/Shared/ilp_efti_connector.Shared.Infrastructure \
  --startup-project src/Services/ilp_efti_connector.ApiGateway

# Script SQL da applicare manualmente (produzione)
dotnet ef migrations script \
  --project src/Shared/ilp_efti_connector.Shared.Infrastructure \
  --startup-project src/Services/ilp_efti_connector.ApiGateway \
  --output migrations.sql \
  --idempotent
```

### Script `scripts/db-migrate.sh`

```bash
#!/usr/bin/env bash
set -euo pipefail

echo "▶ Attendo MariaDB..."
until docker exec ilp-efti-connector-mariadb mariadb-admin ping -u root -p"${MARIADB_ROOT_PASSWORD:-changeme_root}" --silent; do
  sleep 2
done

echo "▶ Eseguo EF Core migrations..."
dotnet ef database update \
  --project src/Shared/ilp-efti-connector.Shared.Infrastructure \
  --startup-project src/Services/ilp-efti-connectorApiGateway \
  --connection "Server=localhost;Port=3306;Database=efti_connector;User=efti_user;Password=changeme_efti;"

echo "✅ Migrations completate."
```

### Configurazione EF Core + Pomelo (Shared.Infrastructure)

```csharp
// EftiConnectorDbContext.cs
public class EftiConnectorDbContext : DbContext
{
    public EftiConnectorDbContext(DbContextOptions<EftiConnectorDbContext> options)
        : base(options) { }

    public DbSet<Customer>                 Customers                  => Set<Customer>();
    public DbSet<CustomerDestination>      CustomerDestinations       => Set<CustomerDestination>();
    public DbSet<Source>                   Sources                    => Set<Source>();
    public DbSet<TransportOperation>       TransportOperations        => Set<TransportOperation>();
    public DbSet<TransportConsignee>       TransportConsignees        => Set<TransportConsignee>();
    public DbSet<TransportCarrier>         TransportCarriers          => Set<TransportCarrier>();
    public DbSet<TransportDetail>          TransportDetails           => Set<TransportDetail>();
    public DbSet<TransportConsignmentItem> TransportConsignmentItems  => Set<TransportConsignmentItem>();
    public DbSet<TransportPackage>         TransportPackages          => Set<TransportPackage>();
    public DbSet<EftiMessage>              EftiMessages               => Set<EftiMessage>();
    public DbSet<User>                     Users                      => Set<User>();
    public DbSet<AuditLog>                 AuditLogs                  => Set<AuditLog>();
}

// InfrastructureExtensions.cs — registra DbContext + tutti i repository
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services, IConfiguration configuration)
{
    services.AddDbContext<EftiConnectorDbContext>(options =>
        options.UseMySql(
            configuration.GetConnectionString("DefaultConnection"),
            new MariaDbServerVersion(new Version(11, 4, 0)),
            mysql => mysql.EnableRetryOnFailure(3)));

    services.AddScoped<ISourceRepository, SourceRepository>();
    services.AddScoped<ICustomerRepository, CustomerRepository>();
    services.AddScoped<ICustomerDestinationRepository, CustomerDestinationRepository>();
    services.AddScoped<ITransportOperationRepository, TransportOperationRepository>();
    services.AddScoped<IEftiMessageRepository, EftiMessageRepository>();
    services.AddScoped<IAuditLogRepository, AuditLogRepository>();
    services.AddScoped<IUnitOfWork, UnitOfWork>();
    return services;
}
```

---

## 6. Message Broker — RabbitMQ

### Code e Exchange

| Exchange | Tipo | Queue | Descrizione |
|---|---|---|---|
| `efti.transport` | topic | `transport.submitted` | Ordine ricevuto da sorgente |
| `efti.transport` | topic | `transport.validated` | Ordine validato |
| `efti.transport` | topic | `transport.validation.failed` | Validazione fallita |
| `efti.gateway` | direct | `gateway.send` | Invio verso MILOS/EFTI |
| `efti.gateway` | direct | `gateway.response` | Risposta da MILOS/EFTI |
| `efti.notifications` | fanout | `notification.webhook` | Notifica sorgente |
| `efti.dlq` | direct | `dead.letter` | Messaggi in errore permanente |

### Configurazione MassTransit (ogni microservizio)

```csharp
// MassTransitExtensions.cs
public static IServiceCollection AddEftiMessaging(
    this IServiceCollection services, IConfiguration configuration)
{
    services.AddMassTransit(x =>
    {
        // Registra i consumer del microservizio corrente
        x.AddConsumersFromNamespaceContaining<TransportSubmittedConsumer>();

        x.UsingRabbitMq((ctx, cfg) =>
        {
            var rabbitCfg = configuration.GetSection("RabbitMQ");
            cfg.Host(rabbitCfg["Host"], rabbitCfg["VirtualHost"], h =>
            {
                h.Username(rabbitCfg["Username"]!);
                h.Password(rabbitCfg["Password"]!);
            });

            cfg.UseMessageRetry(r => r.Exponential(
                retryLimit:     5,
                minInterval:    TimeSpan.FromSeconds(1),
                maxInterval:    TimeSpan.FromMinutes(5),
                intervalDelta:  TimeSpan.FromSeconds(1)));

            cfg.UseDeadLetterQueue("efti.dlq");

            cfg.ConfigureEndpoints(ctx);
        });
    });
    return services;
}
```

---

## 7. Cache — Redis

### Configurazione StackExchange.Redis

```csharp
// RedisExtensions.cs
public static IServiceCollection AddEftiRedis(
    this IServiceCollection services, IConfiguration configuration)
{
    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = configuration["Redis:ConnectionString"];
        options.InstanceName  = "efti:";
    });

    services.AddSingleton<IConnectionMultiplexer>(_ =>
        ConnectionMultiplexer.Connect(configuration["Redis:ConnectionString"]!));

    return services;
}
```

### Chiavi Redis per namespace

```
efti:token:milos                  → Bearer token MILOS (TTL: 3600s)
efti:token:efti-native            → Bearer token EFTI Gate (TTL: 3600s)
efti:ratelimit:{source_id}        → Contatore rate limiting per sorgente
efti:session:{correlation_id}     → Stato sessione elaborazione messaggio
efti:cache:query:{ecmr_id}        → Risposta query autorità (TTL: 300s)
```

---

## 8. Identity Provider — Keycloak

### Configurazione Realm `infra/keycloak/realm-efti.json`

Il file JSON del realm viene esportato direttamente da Keycloak e importato automaticamente al primo avvio del container. Di seguito i punti chiave da configurare manualmente se si ricrea il realm da zero:

**Client `efti-api`** (backend):
- Client type: `confidential`
- Authorization: abilitato
- Service Accounts: abilitato (per OAuth2 client_credentials)

**Client `efti-webapp`** (frontend React):
- Client type: `public`
- Valid redirect URIs: `http://localhost:5173/*` (dev), `https://efti.example.com/*` (prod)
- Web Origins: `+`
- Standard Flow: abilitato

**Ruoli Realm:**
- `efti-operator` — inserimento e-CMR via form, visualizzazione messaggi propri
- `efti-supervisor` — accesso a tutti i messaggi, riprocessamento DLQ
- `efti-admin` — gestione utenti, sorgenti, switch provider MILOS/EFTI
- `efti-service` — account di servizio per microservizi M2M

### Configurazione ASP.NET Core JWT Bearer

```csharp
// AuthExtensions.cs
public static IServiceCollection AddEftiAuth(
    this IServiceCollection services, IConfiguration configuration)
{
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = configuration["Keycloak:Authority"];
            options.Audience  = configuration["Keycloak:Audience"];
            options.RequireHttpsMetadata = false; // true in produzione
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer   = true,
                ValidateAudience = true,
                ClockSkew        = TimeSpan.FromSeconds(30)
            };
        });

    services.AddAuthorization(options =>
    {
        options.AddPolicy("RequireOperator",  p => p.RequireRole("efti-operator",  "efti-supervisor", "efti-admin"));
        options.AddPolicy("RequireSupervisor",p => p.RequireRole("efti-supervisor", "efti-admin"));
        options.AddPolicy("RequireAdmin",     p => p.RequireRole("efti-admin"));
    });

    return services;
}
```

### Script `scripts/keycloak-import.sh`

```bash
#!/usr/bin/env bash
set -euo pipefail

echo "▶ Attendo Keycloak..."
until curl -sf http://localhost:8080/health/ready > /dev/null; do sleep 3; done

echo "✅ Keycloak pronto — il realm 'efti' viene importato automaticamente all'avvio."
echo "   Admin console: http://localhost:8080  (admin / changeme_kc)"
echo ""
echo "   Se serve reimportare manualmente:"
echo "   docker exec ilp-efti-connector-keycloak /opt/keycloak/bin/kc.sh import --file /opt/keycloak/data/import/realm-efti.json"
```

---

## 9. API Gateway — YARP

### Progetto `ilp-efti-connectorApiGateway`

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddEftiAuth(builder.Configuration);
builder.Services.AddEftiRedis(builder.Configuration);  // per rate limiting

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();
app.Run();
```

### `appsettings.json` — sezione YARP

```json
{
  "ReverseProxy": {
    "Routes": {
      "validation-route": {
        "ClusterId": "validation-cluster",
        "AuthorizationPolicy": "default",
        "Match": { "Path": "/api/v1/transport{**catch-all}" }
      },
      "form-route": {
        "ClusterId": "form-cluster",
        "AuthorizationPolicy": "RequireOperator",
        "Match": { "Path": "/api/v1/form{**catch-all}" }
      },
      "admin-route": {
        "ClusterId": "form-cluster",
        "AuthorizationPolicy": "RequireAdmin",
        "Match": { "Path": "/api/v1/admin{**catch-all}" }
      }
    },
    "Clusters": {
      "validation-cluster": {
        "Destinations": {
          "primary": { "Address": "http://efti-validation-service:8080" }
        }
      },
      "form-cluster": {
        "Destinations": {
          "primary": { "Address": "http://efti-form-service:8080" }
        }
      }
    }
  }
}
```

---

## 10. Logging — Serilog + Seq

### Configurazione Serilog (ogni microservizio)

```csharp
// Program.cs
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProperty("Service", "ilp-efti-connectorValidationService")
    .WriteTo.Console(new JsonFormatter())
    .WriteTo.Seq(ctx.Configuration["Seq:ServerUrl"]!));
```

### `appsettings.json` — sezione Serilog

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

### Middleware per `correlation_id`

```csharp
// CorrelationIdMiddleware.cs
public class CorrelationIdMiddleware
{
    private const string Header = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[Header].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();
        context.Response.Headers[Header] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
            await _next(context);
    }
}
```

---

## 11. Monitoring — Prometheus + Grafana

### `infra/prometheus/prometheus.yml`

```yaml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

scrape_configs:
  - job_name: "efti-api-gateway"
    static_configs:
      - targets: ["efti-api-gateway:8080"]
    metrics_path: /metrics

  - job_name: "efti-validation-service"
    static_configs:
      - targets: ["efti-validation-service:8080"]

  - job_name: "efti-gateway-service"
    static_configs:
      - targets: ["efti-efti-gateway-service:8080"]

  - job_name: "rabbitmq"
    static_configs:
      - targets: ["efti-rabbitmq:15692"]

  - job_name: "mariadb"
    static_configs:
      - targets: ["efti-mariadb-exporter:9104"]
```

### Configurazione Prometheus (ogni microservizio)

```csharp
// Program.cs
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation();
        metrics.AddHttpClientInstrumentation();
        metrics.AddRuntimeInstrumentation();
        metrics.AddMeter("ilp-efti-connector*");
        metrics.AddPrometheusExporter();
    })
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation();
        tracing.AddHttpClientInstrumentation();
        tracing.AddEntityFrameworkCoreInstrumentation();
        tracing.AddSource("MassTransit");
        tracing.AddOtlpExporter();
    });

app.MapPrometheusScrapingEndpoint("/metrics");
```

### Metriche custom da implementare

```csharp
// IlpEftiMetrics.cs
public class IlpEftiMetrics
{
    private readonly Counter<long>   _messagesSent;
    private readonly Counter<long>   _messagesAcknowledged;
    private readonly Counter<long>   _messagesFailed;
    private readonly Histogram<double> _gatewayLatency;

    public IlpEftiMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("ilp-efti-connector.Gateway");
        _messagesSent         = meter.CreateCounter<long>("efti.messages.sent");
        _messagesAcknowledged = meter.CreateCounter<long>("efti.messages.acknowledged");
        _messagesFailed       = meter.CreateCounter<long>("efti.messages.failed");
        _gatewayLatency       = meter.CreateHistogram<double>(
            "efti.gateway.latency", "ms", "Latenza verso gateway esterno");
    }

    public void RecordSent(string provider) =>
        _messagesSent.Add(1, new("provider", provider));

    public void RecordLatency(double ms, string provider) =>
        _gatewayLatency.Record(ms, new("provider", provider));
}
```

---

## 12. Gateway EFTI — Fase 1 (MILOS) e Fase 2 (EFTI Native)

### Selezione provider a runtime

```csharp
// EftiGatewayService/Program.cs
// Entrambi i gateway vengono registrati; GatewaySelector risolve a runtime
// il provider attivo leggendo EftiGateway:Provider da appsettings/env.
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMilosGateway(builder.Configuration);       // Fase 1
builder.Services.AddEftiNativeGateway(builder.Configuration);  // Fase 2
builder.Services.AddScoped<GatewaySelector>();                 // risolve per nome
builder.Services.AddHostedService<GatewayHealthMonitor>();     // ping ogni 60 s

builder.Services.AddIlpEftiMessaging(builder.Configuration, x =>
{
    x.AddConsumer<EftiSendRequestedConsumer>();
    x.AddConsumer<SendToGatewayConsumer>();
});

var host = builder.Build();
host.Run();
```

> Il `GatewaySelector` (Scoped) espone `Get("MILOS")` / `Get("EFTI_NATIVE")`. Il consumer legge il provider attivo da `EftiGateway:Provider` e delega la chiamata all'implementazione corretta tramite `IEftiGateway`.

### Fase 1 — Mock MILOS per sviluppo locale

La directory `infra/docker/milos-mock/mappings/` contiene le stub WireMock per simulare le risposte MILOS:

```json
// mappings/ecmr-create.json
{
  "request": {
    "method": "POST",
    "urlPath": "/api/ecmr-service/ecmr"
  },
  "response": {
    "status": 200,
    "headers": { "Content-Type": "application/json" },
    "jsonBody": {
      "eCMRID": "MOCK-{{randomValue type='UUID'}}",
      "uuid":   "{{randomValue type='UUID'}}",
      "consignorSender": {
        "name": "Mock Mittente S.r.l."
      }
    }
  }
}
```

```json
// mappings/ecmr-update.json
{
  "request": { "method": "PUT", "urlPathPattern": "/api/ecmr-service/ecmr/.*" },
  "response": { "status": 200, "body": "OK" }
}
```

```json
// mappings/ecmr-delete.json
{
  "request": { "method": "DELETE", "urlPathPattern": "/api/ecmr-service/ecmr/.*" },
  "response": { "status": 200, "body": "OK" }
}
```

### Fase 2 — Configurazione X.509 e OAuth2

```csharp
// EftiNativeGatewayExtensions.cs
public static IServiceCollection AddEftiNativeGateway(
    this IServiceCollection services, IConfiguration configuration)
{
    services.Configure<EftiNativeOptions>(configuration.GetSection("EftiGateway:EftiNative"));

    services.AddIlpEftiRedis(configuration);       // token cache Redis (TTL 1h)
    services.AddSingleton<EftiTokenCache>();
    services.AddTransient<EftiOAuth2Handler>();

    services.AddRefitClient<IEftiGateClient>()
        .ConfigureHttpClient((sp, c) =>
        {
            var opts = sp.GetRequiredService<IOptions<EftiNativeOptions>>().Value;
            c.BaseAddress = new Uri(opts.BaseUrl);
            c.Timeout     = TimeSpan.FromSeconds(opts.TimeoutSeconds);
        })
        // ① Polly v8: Retry(3, exp+jitter) → CircuitBreaker(50%, break=30s) → Timeout(30s)
        .AddHttpMessageHandler(_ => new GatewayResilienceHandler(ResiliencePolicies.CreateGatewayPipeline()))
        // ② OAuth2 Bearer token da cache Redis
        .AddHttpMessageHandler<EftiOAuth2Handler>();

    services.AddScoped<IEftiGateway, EftiNativeGateway>();
    return services;
}
```

### Polly v8 — GatewayResilienceHandler + ResiliencePolicies

```csharp
// Shared.Infrastructure/Resilience/GatewayResilienceHandler.cs
// DelegatingHandler che avvolge ogni chiamata HTTP in uscita nel pipeline Polly v8.
public sealed class GatewayResilienceHandler : DelegatingHandler
{
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

    public GatewayResilienceHandler(ResiliencePipeline<HttpResponseMessage> pipeline)
        => _pipeline = pipeline;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
        => await _pipeline.ExecuteAsync(
            async token => await base.SendAsync(request, token), ct);
}

// Shared.Infrastructure/Resilience/ResiliencePolicies.cs
public static class ResiliencePolicies
{
    /// <summary>Pipeline gateway: Retry(3, exp+jitter) → CircuitBreaker(50%, break=30s) → Timeout(30s).</summary>
    public static ResiliencePipeline<HttpResponseMessage> CreateGatewayPipeline(
        int retryCount = 3, int circuitBreakSeconds = 30, int timeoutSeconds = 30) =>
        new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = retryCount,
                BackoffType      = DelayBackoffType.Exponential,
                Delay            = TimeSpan.FromSeconds(1),
                UseJitter        = true,
                ShouldHandle     = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(r => (int)r.StatusCode >= 500)
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                FailureRatio      = 0.5,
                SamplingDuration  = TimeSpan.FromSeconds(circuitBreakSeconds),
                MinimumThroughput = 5,
                BreakDuration     = TimeSpan.FromSeconds(circuitBreakSeconds),
                ShouldHandle      = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(r => (int)r.StatusCode >= 500)
            })
            .AddTimeout(TimeSpan.FromSeconds(timeoutSeconds))
            .Build();
}
```

> Il `GatewayResilienceHandler` viene registrato con una factory lambda (`.AddHttpMessageHandler(_ => new GatewayResilienceHandler(ResiliencePolicies.CreateGatewayPipeline()))`) **prima** dell'handler di autenticazione (`MilosApiKeyHandler` / `EftiOAuth2Handler`) sia in `MilosGatewayExtensions` che in `EftiNativeGatewayExtensions`.

---

## 13. CI/CD Pipeline

### GitHub Actions — `.github/workflows/ci.yml`

```yaml
name: CI

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

env:
  DOTNET_VERSION: "9.0.x"
  REGISTRY: ghcr.io
  IMAGE_PREFIX: alisandre/ilp_efti_connector

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    services:
      mariadb:
        image: mariadb:11.4
        env:
          MARIADB_ROOT_PASSWORD: test_root
          MARIADB_DATABASE: efti_test
          MARIADB_USER: efti_test
          MARIADB_PASSWORD: test_pass
        ports: ["3306:3306"]
        options: --health-cmd="healthcheck.sh --connect --innodb_initialized" --health-interval=10s --health-retries=5

      rabbitmq:
        image: rabbitmq:3.13-management
        env:
          RABBITMQ_DEFAULT_USER: efti_rabbit
          RABBITMQ_DEFAULT_PASS: test_rabbit
          RABBITMQ_DEFAULT_VHOST: efti
        ports: ["5672:5672"]
        options: --health-cmd="rabbitmq-diagnostics ping" --health-interval=10s

      redis:
        image: redis:7-alpine
        ports: ["6379:6379"]
        options: --health-cmd="redis-cli ping" --health-interval=5s

    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore --configuration Release

      - name: Unit Tests
        run: dotnet test tests/ilp-efti-connector.Domain.Tests tests/ilp-efti-connector.Application.Tests \
               --no-build --configuration Release --logger trx --results-directory TestResults

      - name: Integration Tests
        env:
          ConnectionStrings__DefaultConnection: "Server=localhost;Port=3306;Database=efti_test;User=efti_test;Password=test_pass;"
          RabbitMQ__Host: localhost
          Redis__ConnectionString: "localhost:6379"
        run: dotnet test tests/ilp-efti-connectorIntegrationTests \
               --no-build --configuration Release --logger trx --results-directory TestResults

      - name: Publish Test Results
        uses: dorny/test-reporter@v1
        if: always()
        with:
          name: Test Results
          path: TestResults/*.trx
          reporter: dotnet-trx

  docker-build-push:
    needs: build-and-test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    strategy:
      matrix:
        service:
          - apigateway
          - validationservice
          - normalizationservice
          - eftigatewayservice
          - responsehandlerservice
          - notificationservice
          - retryservice
          - forminputservice
    steps:
      - uses: actions/checkout@v4

      - name: Login to GHCR
        uses: docker/login-action@v3
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Build and push ${{ matrix.service }}
        uses: docker/build-push-action@v5
        with:
          context: .
          file: infra/docker/dockerfiles/Dockerfile.${{ matrix.service }}
          push: true
          tags: ${{ env.REGISTRY }}/${{ env.IMAGE_PREFIX }}/${{ matrix.service }}:${{ github.sha }},${{ env.REGISTRY }}/${{ env.IMAGE_PREFIX }}/${{ matrix.service }}:latest
          cache-from: type=gha
          cache-to: type=gha,mode=max

  deploy-staging:
    needs: docker-build-push
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
      - uses: actions/checkout@v4

      - name: Deploy to staging
        run: |
          helm upgrade --install efti-connector infra/k8s/helm/efti-connector \
            --namespace efti-staging \
            --values infra/k8s/helm/efti-connector/values.staging.yaml \
            --set global.imageTag=${{ github.sha }} \
            --wait --timeout 5m
```

### Dockerfile esempio `infra/docker/dockerfiles/Dockerfile.eftigatewayservice`

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia solution e csproj per layer caching
COPY ilp-efti-connectorsln .
COPY src/Core/ilp-efti-connector.Domain/ilp-efti-connector.Domain.csproj                   src/Core/ilp-efti-connector.Domain/
COPY src/Core/ilp-efti-connector.Application/ilp-efti-connector.Application.csproj         src/Core/ilp-efti-connector.Application/
COPY src/Shared/ilp-efti-connector.Shared.Contracts/ilp-efti-connector.Shared.Contracts.csproj src/Shared/ilp-efti-connector.Shared.Contracts/
COPY src/Shared/ilp-efti-connector.Shared.Infrastructure/ilp-efti-connector.Shared.Infrastructure.csproj src/Shared/ilp-efti-connector.Shared.Infrastructure/
COPY src/Gateway/ilp-efti-connector.Gateway.Contracts/ilp-efti-connector.Gateway.Contracts.csproj src/Gateway/ilp-efti-connector.Gateway.Contracts/
COPY src/Gateway/ilp-efti-connector.Gateway.Milos/ilp-efti-connector.Gateway.Milos.csproj src/Gateway/ilp-efti-connector.Gateway.Milos/
COPY src/Gateway/ilp-efti-connector.Gateway.EftiNative/ilp-efti-connector.Gateway.EftiNative.csproj src/Gateway/ilp-efti-connector.Gateway.EftiNative/
COPY src/Services/ilp-efti-connectorEftiGatewayService/ilp-efti-connectorEftiGatewayService.csproj src/Services/ilp-efti-connectorEftiGatewayService/

RUN dotnet restore src/Services/ilp-efti-connectorEftiGatewayService/ilp-efti-connectorEftiGatewayService.csproj

COPY . .
RUN dotnet publish src/Services/ilp-efti-connectorEftiGatewayService \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ilp-efti-connectorEftiGatewayService.dll"]
```

---

## 14. Produzione — Kubernetes

### Namespace `infra/k8s/namespaces.yaml`

```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: efti-staging
  labels:
    environment: staging
---
apiVersion: v1
kind: Namespace
metadata:
  name: efti-production
  labels:
    environment: production
```

### Helm Chart — `infra/k8s/helm/efti-connector/values.yaml`

```yaml
global:
  imageRegistry: ghcr.io/alisandre/ilp_efti_connector
  imageTag: latest
  imagePullPolicy: Always

# ── Gateway Provider (Fase 1: Milos / Fase 2: EftiNative) ─────
eftiGateway:
  provider: Milos   # cambia in EftiNative per Fase 2

# ── Replicas per microservizio ──────────────────────────────────
services:
  apiGateway:
    replicaCount: 2
    resources:
      requests: { cpu: 100m, memory: 128Mi }
      limits:   { cpu: 500m, memory: 512Mi }

  validationService:
    replicaCount: 2
    resources:
      requests: { cpu: 200m, memory: 256Mi }
      limits:   { cpu: 1000m, memory: 1Gi }

  normalizationService:
    replicaCount: 2
    resources:
      requests: { cpu: 200m, memory: 256Mi }
      limits:   { cpu: 1000m, memory: 1Gi }

  eftiGatewayService:
    replicaCount: 2
    resources:
      requests: { cpu: 100m, memory: 128Mi }
      limits:   { cpu: 500m, memory: 512Mi }

  responseHandlerService:
    replicaCount: 1
    resources:
      requests: { cpu: 100m, memory: 128Mi }
      limits:   { cpu: 500m, memory: 256Mi }

  notificationService:
    replicaCount: 1
    resources:
      requests: { cpu: 100m, memory: 128Mi }
      limits:   { cpu: 500m, memory: 256Mi }

  retryService:
    replicaCount: 1
    resources:
      requests: { cpu: 100m, memory: 128Mi }
      limits:   { cpu: 500m, memory: 256Mi }

  formInputService:
    replicaCount: 2
    resources:
      requests: { cpu: 100m, memory: 256Mi }
      limits:   { cpu: 500m, memory: 512Mi }

# ── HPA ─────────────────────────────────────────────────────────
hpa:
  enabled: true
  minReplicas: 2
  maxReplicas: 10
  targetCPUUtilizationPercentage: 70

# ── MariaDB (Galera Cluster) ─────────────────────────────────────
mariadb:
  galera:
    enabled: true
    replicaCount: 3
  auth:
    database: efti_connector
    username: efti_user
    existingSecret: efti-mariadb-secret

# ── RabbitMQ ──────────────────────────────────────────────────────
rabbitmq:
  replicaCount: 3
  auth:
    existingPasswordSecret: efti-rabbitmq-secret

# ── Redis ─────────────────────────────────────────────────────────
redis:
  architecture: sentinel
  sentinel:
    enabled: true
  auth:
    existingSecret: efti-redis-secret

# ── Ingress ───────────────────────────────────────────────────────
ingress:
  enabled: true
  className: nginx
  annotations:
    cert-manager.io/cluster-issuer: letsencrypt-prod
  hosts:
    - host: efti-api.example.com
      paths:
        - path: /
          pathType: Prefix
  tls:
    - secretName: efti-tls
      hosts: [efti-api.example.com]
```

### Passaggio Fase 1 → Fase 2 (zero-downtime)

```bash
# Aggiorna solo il provider nel values — rolling update automatico
helm upgrade efti-connector infra/k8s/helm/efti-connector \
  --namespace efti-production \
  --reuse-values \
  --set eftiGateway.provider=EftiNative \
  --wait --timeout 5m

# Verifica rollout
kubectl rollout status deployment/efti-eftigatewayservice -n efti-production

# Rollback immediato se necessario
helm rollback efti-connector --namespace efti-production
```

---

## 15. Secrets Management

### In sviluppo locale

Usare il file `infra/docker/.env` (mai committato in git — aggiunto a `.gitignore`).

```
# .gitignore
infra/docker/.env
*.pfx
*.p12
certs/
```

### In produzione (Kubernetes + HashiCorp Vault)

```bash
# Crea i secrets Kubernetes (popolati da Vault Agent o External Secrets Operator)
kubectl create secret generic efti-mariadb-secret \
  --from-literal=mariadb-password="$(vault kv get -field=password secret/efti/mariadb)" \
  --namespace efti-production

kubectl create secret generic efti-milos-secret \
  --from-literal=api-key="$(vault kv get -field=api-key secret/efti/milos)" \
  --namespace efti-production

# Per Fase 2 — certificato X.509 EFTI
kubectl create secret tls efti-x509-cert \
  --cert=certs/efti-client.crt \
  --key=certs/efti-client.key \
  --namespace efti-production
```

### .NET User Secrets (sviluppo senza Docker)

```bash
# Inizializza User Secrets per il progetto di startup
dotnet user-secrets init --project src/Services/ilp-efti-connectorApiGateway

# Imposta i secrets
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;..." \
  --project src/Services/ilp-efti-connectorApiGateway

dotnet user-secrets set "EftiGateway:Milos:ApiKey" "your-api-key" \
  --project src/Services/ilp-efti-connectorEftiGatewayService
```

---

## 16. Health Checks e Readiness

### Configurazione (ogni microservizio)

```csharp
// HealthCheckExtensions.cs
public static IServiceCollection AddEftiHealthChecks(
    this IServiceCollection services, IConfiguration configuration)
{
    services.AddHealthChecks()
        .AddMySql(
            configuration.GetConnectionString("DefaultConnection")!,
            name: "mariadb",
            tags: ["db", "ready"])
        .AddRabbitMQ(
            sp => sp.GetRequiredService<IConnectionFactory>(),
            name: "rabbitmq",
            tags: ["messaging", "ready"])
        .AddRedis(
            configuration["Redis:ConnectionString"]!,
            name: "redis",
            tags: ["cache", "ready"])
        .AddUrlGroup(
            new Uri($"{configuration["Keycloak:Authority"]}/.well-known/openid-configuration"),
            name: "keycloak",
            tags: ["auth"]);
    return services;
}

// Program.cs
app.MapHealthChecks("/health/live",  new() { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new() { Predicate = hc => hc.Tags.Contains("ready") });
app.MapHealthChecks("/health",       new() { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });
```

### GatewayHealthMonitor (solo EftiGatewayService)

`GatewayHealthMonitor` è un `BackgroundService` registrato unicamente in `EftiGatewayService`. Ogni **60 secondi** chiama `HealthCheckAsync()` su tutti i provider registrati (`MILOS`, `EFTI_NATIVE`) tramite `GatewaySelector` e logga il risultato:

```csharp
// Registrazione in EftiGatewayService/Program.cs
builder.Services.AddHostedService<GatewayHealthMonitor>();

// Output log (Serilog/Seq)
// [INF] Gateway MILOS       — HEALTHY  (42 ms)
// [WRN] Gateway EFTI_NATIVE — UNHEALTHY: Connection refused
```

I log sono consultabili in Seq con filtro `Source = "GatewayHealthMonitor"` e integrabili in Grafana tramite il Loki datasource.

---

## 17. Runbook Operativo

### Comandi di uso quotidiano

```bash
# ── Stato dello stack ────────────────────────────────────────
docker compose ps                          # stato container locali
kubectl get pods -n efti-production        # stato pod in produzione

# ── Log in tempo reale ───────────────────────────────────────
docker logs -f efti-validation-service
kubectl logs -f deployment/efti-eftigatewayservice -n efti-production
# oppure: http://localhost:8888 (Seq) con filtro CorrelationId

# ── Riavvio di un servizio ───────────────────────────────────
docker compose restart efti-gateway-service
kubectl rollout restart deployment/efti-eftigatewayservice -n efti-production

# ── Gestione DLQ ─────────────────────────────────────────────
# Visualizza messaggi in coda dead letter
docker exec ilp-efti-connector-rabbitmq rabbitmqadmin list queues name messages

# Svuota DLQ (ATTENZIONE: messaggi persi)
docker exec ilp-efti-connector-rabbitmq rabbitmqadmin purge queue name=dead.letter

# ── Switch provider MILOS → EFTI Native (produzione) ─────────
helm upgrade efti-connector infra/k8s/helm/efti-connector \
  --namespace efti-production --reuse-values \
  --set eftiGateway.provider=EftiNative --wait

# ── Rollback ultimo deploy ────────────────────────────────────
helm rollback efti-connector --namespace efti-production

# ── Backup database ──────────────────────────────────────────
docker exec ilp-efti-connector-mariadb mariadb-dump \
  -u root -p"${MARIADB_ROOT_PASSWORD}" \
  --all-databases > backup-$(date +%Y%m%d-%H%M%S).sql

# ── Verifica certificato X.509 EFTI (Fase 2) ─────────────────
openssl x509 -in certs/efti-client.crt -noout -dates
```

### Checklist go-live Fase 1 (MILOS)

```
[ ] Stack infrastruttura avviato e healthy
[ ] Migrations database applicate (dotnet ef database update)
[ ] Realm Keycloak importato e utenti di test creati
[ ] API Key MILOS (sandbox) configurata in Vault / .env
[ ] Mock MILOS sostituito con URL sandbox reale (MILOS_BASE_URL)
[ ] Test end-to-end manuale: POST /api/v1/transport → verifica eCMRID in risposta
[ ] Health checks verdi: /health/ready su tutti i microservizi
[ ] Dashboard Grafana: metriche in ingresso
[ ] Seq: log fluenti, nessun errore critico
[ ] Notifiche webhook sorgente: almeno un ciclo completato
```

### Checklist go-live Fase 2 (EFTI Native)

```
[ ] Certificato X.509 ottenuto da CA accreditata EU e caricato in Vault
[ ] Client ID e segreto OAuth2 EFTI configurati in Vault
[ ] Test autenticazione OAuth2 verso EFTI Gate (token ottenuto correttamente)
[ ] Test AS4 su ambiente sandbox EFTI Gate nazionale
[ ] EftiNativeGateway testato end-to-end (invio eCMR → ACK)
[ ] Query Proxy Service abilitato e testato con simulazione query autorità
[ ] Helm values aggiornati: eftiGateway.provider = EftiNative (staging)
[ ] Regression test Fase 1: path MILOS ancora funzionante (fallback verificato)
[ ] Rolling update eseguito su staging, zero errori per 24h
[ ] Helm upgrade produzione con --wait e monitoraggio attivo
[ ] Grafana: nessun alert attivato nei 30 minuti post-deploy
```

---

*EFTI Connector Platform — Infrastructure Guide v1.1 — Febbario 2026*  
*Aggiornato: §5 `EftiConnectorDbContext` (12 DbSet, `AddInfrastructure` con tutti i repository), §12 `GatewayResilienceHandler` Polly v8 + `GatewaySelector`, §16 `GatewayHealthMonitor` BackgroundService*

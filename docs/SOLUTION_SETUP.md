# EFTI Connector Platform — Solution Setup Guide

> **Versione:** 1.0 · **Data:** Febbraio 2026  
> Guida passo-passo per creare la solution C# e tutti i progetti che la compongono.  
> Da collocare nella root della solution: `SOLUTION_SETUP.md`

---

## Indice

1. [Creazione della Solution](#1-creazione-della-solution)
2. [Progetti Core (Domain / Application / Infrastructure)](#2-progetti-core)
3. [Progetti Shared](#3-progetti-shared)
4. [Progetti Gateway (bifase)](#4-progetti-gateway)
5. [Microservizi (host)](#5-microservizi-host)
6. [Progetti di Test](#6-progetti-di-test)
7. [Configurazione Package References](#7-configurazione-package-references)
8. [Struttura interna di ogni progetto](#8-struttura-interna-di-ogni-progetto)
9. [Verifica finale della solution](#9-verifica-finale-della-solution)

---

## 1. Creazione della Solution

```bash
# Crea la cartella radice e la solution
mkdir ilp_efti_connector && cd ilp_efti_connector
dotnet new sln -n ilp_efti_connector

# Crea la struttura di cartelle
mkdir -p src/Core
mkdir -p src/Gateway
mkdir -p src/Services
mkdir -p src/Shared
mkdir -p tests
mkdir -p infra/docker
mkdir -p infra/keycloak
mkdir -p infra/prometheus
mkdir -p infra/grafana/provisioning/datasources
mkdir -p infra/grafana/provisioning/dashboards
mkdir -p infra/mariadb/init
mkdir -p infra/k8s/helm/efti-connector/templates
mkdir -p scripts
mkdir -p frontend
```

---

## 2. Progetti Core

I progetti Core implementano il cuore del dominio applicativo. **Non hanno dipendenze verso infrastruttura** (nessun EF Core, nessun HTTP client, nessun RabbitMQ).

### 2.1 — `EftiConnector.Domain`

Contiene entità, value objects, enum di dominio, interfacce dei repository e domain exceptions. **Zero dipendenze NuGet.**

```bash
dotnet new classlib -n ilp_efti_connector.Domain \
  -o src/Core/ilp_efti_connector.Domain \
  -f net9.0

dotnet sln add src/Core/ilp_efti_connector.Domain/ilp_efti_connector.Domain.csproj
```

**Struttura interna:**

```
ilp_efti_connector.Domain/
├── Entities/
│   ├── Customer.cs
│   ├── CustomerDestination.cs
│   ├── Source.cs
│   ├── TransportOperation.cs
│   ├── EftiMessage.cs
│   └── AuditLog.cs
├── ValueObjects/
│   ├── EoriCode.cs
│   ├── CountryCode.cs
│   ├── PostalAddress.cs
│   └── VatNumber.cs
├── Enums/
│   ├── GatewayProvider.cs
│   ├── EftiMessageStatus.cs
│   ├── DatasetType.cs
│   ├── TransportMode.cs
│   ├── SourceType.cs
│   └── TransportOperationStatus.cs
├── Interfaces/
│   └── Repositories/
│       ├── ICustomerRepository.cs
│       ├── ICustomerDestinationRepository.cs
│       ├── ISourceRepository.cs
│       ├── ITransportOperationRepository.cs
│       ├── IEftiMessageRepository.cs
│       └── IAuditLogRepository.cs
└── Exceptions/
    ├── DomainException.cs
    ├── CustomerNotFoundException.cs
    └── DuplicateOperationCodeException.cs
```

**Esempi di file chiave:**

```csharp
// Enums/GatewayProvider.cs
namespace ilp_efti_connector.Domain.Enums;

public enum GatewayProvider
{
    Milos,
    EftiNative
}
```

```csharp
// Enums/EftiMessageStatus.cs
namespace ilp_efti_connector.Domain.Enums;

public enum EftiMessageStatus
{
    Pending,
    Sent,
    Acknowledged,
    Error,
    Retry,
    Dead
}
```

```csharp
// Entities/Customer.cs
namespace ilp_efti_connector.Domain.Entities;

public class Customer
{
    public Guid   Id             { get; private set; } = Guid.NewGuid();
    public string CustomerCode   { get; private set; } = default!;
    public string BusinessName   { get; private set; } = default!;
    public string? VatNumber     { get; private set; }
    public string? EoriCode      { get; private set; }
    public string? ContactEmail  { get; private set; }
    public bool   IsActive       { get; private set; } = true;
    public bool   AutoCreated    { get; private set; }
    public Guid?  SourceId       { get; private set; }
    public DateTime CreatedAt    { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt    { get; private set; } = DateTime.UtcNow;

    public ICollection<CustomerDestination> Destinations { get; private set; } = [];
    public ICollection<TransportOperation>  Operations   { get; private set; } = [];

    // EF Core constructor
    private Customer() { }

    public static Customer Create(string customerCode, string businessName,
        string? vatNumber, string? eoriCode, bool autoCreated, Guid? sourceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customerCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(businessName);

        return new Customer
        {
            CustomerCode = customerCode,
            BusinessName = businessName,
            VatNumber    = vatNumber,
            EoriCode     = eoriCode,
            AutoCreated  = autoCreated,
            SourceId     = sourceId
        };
    }

    public void Update(string businessName, string? vatNumber, string? eoriCode)
    {
        BusinessName = businessName;
        VatNumber    = vatNumber;
        EoriCode     = eoriCode;
        UpdatedAt    = DateTime.UtcNow;
    }
}
```

```csharp
// ValueObjects/EoriCode.cs
namespace ilp_efti_connector.Domain.ValueObjects;

public record EoriCode
{
    public string Value { get; }

    public EoriCode(string value)
    {
        // formato: 2 lettere ISO paese + fino a 15 caratteri alfanumerici
        if (string.IsNullOrWhiteSpace(value) || value.Length < 3 || value.Length > 17)
            throw new ArgumentException($"EORI code non valido: {value}", nameof(value));
        Value = value.ToUpperInvariant();
    }

    public static implicit operator string(EoriCode code) => code.Value;
    public override string ToString() => Value;
}
```

---

### 2.2 — `EftiConnector.Application`

Contiene use cases (CQRS: Commands, Queries, Handlers), interfacce dei servizi applicativi e DTO. Dipende solo da `Domain`.

```bash
dotnet new classlib -n ilp_efti_connector.Application \
  -o src/Core/ilp_efti_connector.Application \
  -f net9.0

dotnet sln add src/Core/ilp_efti_connector.Application/ilp_efti_connector.Application.csproj

# Aggiunge riferimento a Domain
dotnet add src/Core/ilp_efti_connector.Application/ilp_efti_connector.Application.csproj \
  reference src/Core/ilp_efti_connector.Domain/ilp_efti_connector.Domain.csproj
```

**Package NuGet:**

```bash
cd src/Core/ilp_efti_connector.Application

dotnet add package MediatR --version 12.*
dotnet add package FluentValidation --version 11.*
dotnet add package FluentValidation.DependencyInjectionExtensions --version 11.*
dotnet add package Mapster --version 7.*
dotnet add package Mapster.DependencyInjection --version 1.*
```

**Struttura interna:**

```
ilp_efti_connector.Application/
├── Common/
│   ├── Interfaces/
│   │   ├── ICurrentUserService.cs
│   │   ├── IDateTimeProvider.cs
│   │   └── ICustomerUpsertService.cs
│   ├── Behaviours/
│   │   ├── ValidationBehaviour.cs
│   │   └── LoggingBehaviour.cs
│   └── Exceptions/
│       └── ValidationException.cs
│
├── Customers/
│   ├── Commands/
│   │   ├── UpsertCustomer/
│   │   │   ├── UpsertCustomerCommand.cs
│   │   │   ├── UpsertCustomerCommandHandler.cs
│   │   │   └── UpsertCustomerCommandValidator.cs
│   │   └── UpdateCustomer/
│   │       ├── UpdateCustomerCommand.cs
│   │       └── UpdateCustomerCommandHandler.cs
│   └── Queries/
│       ├── GetCustomerByCode/
│       │   ├── GetCustomerByCodeQuery.cs
│       │   └── GetCustomerByCodeQueryHandler.cs
│       └── GetCustomersWithAutoCreated/
│           ├── GetCustomersWithAutoCreatedQuery.cs
│           └── GetCustomersWithAutoCreatedQueryHandler.cs
│
├── TransportOperations/
│   ├── Commands/
│   │   ├── SubmitTransportOperation/
│   │   │   ├── SubmitTransportOperationCommand.cs
│   │   │   ├── SubmitTransportOperationCommandHandler.cs
│   │   │   └── SubmitTransportOperationCommandValidator.cs
│   │   └── CreateDraftTransportOperation/
│   │       ├── CreateDraftCommand.cs
│   │       └── CreateDraftCommandHandler.cs
│   └── Queries/
│       └── GetTransportOperation/
│           ├── GetTransportOperationQuery.cs
│           └── GetTransportOperationQueryHandler.cs
│
├── EftiMessages/
│   ├── Commands/
│   │   └── RetryEftiMessage/
│   │       ├── RetryEftiMessageCommand.cs
│   │       └── RetryEftiMessageCommandHandler.cs
│   └── Queries/
│       └── GetEftiMessages/
│           ├── GetEftiMessagesQuery.cs
│           └── GetEftiMessagesQueryHandler.cs
│
└── DTOs/
    ├── CustomerDto.cs
    ├── CustomerDestinationDto.cs
    ├── TransportOperationDto.cs
    └── EftiMessageDto.cs
```

---

### 2.3 — `EftiConnector.Infrastructure`

Implementa i repository EF Core, l'audit interceptor e i servizi applicativi. Dipende da `Domain` e `Application`.

```bash
dotnet new classlib -n ilp_efti_connector.Infrastructure \
  -o src/Core/ilp_efti_connector.Infrastructure \
  -f net9.0

dotnet sln add src/Core/ilp_efti_connector.Infrastructure/ilp_efti_connector.Infrastructure.csproj

dotnet add src/Core/ilp_efti_connector.Infrastructure/ilp_efti_connector.Infrastructure.csproj \
  reference src/Core/ilp_efti_connector.Domain/ilp_efti_connector.Domain.csproj

dotnet add src/Core/ilp_efti_connector.Infrastructure/ilp_efti_connector.Infrastructure.csproj \
  reference src/Core/ilp_efti_connector.Application/ilp_efti_connector.Application.csproj
```

**Package NuGet:**

```bash
cd src/Core/ilp_efti_connector.Infrastructure

dotnet add package Microsoft.EntityFrameworkCore --version 9.*
dotnet add package Pomelo.EntityFrameworkCore.MySql --version 9.*
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.*
```

**Struttura interna:**

```
ilp_efti_connector.Infrastructure/
├── Persistence/
│   ├── IlpEftiDbContext.cs
│   ├── Configurations/                  ← IEntityTypeConfiguration<T> per ogni entità
│   │   ├── CustomerConfiguration.cs
│   │   ├── CustomerDestinationConfiguration.cs
│   │   ├── SourceConfiguration.cs
│   │   ├── TransportOperationConfiguration.cs
│   │   ├── EftiMessageConfiguration.cs
│   │   └── AuditLogConfiguration.cs
│   ├── Repositories/
│   │   ├── CustomerRepository.cs
│   │   ├── CustomerDestinationRepository.cs
│   │   ├── TransportOperationRepository.cs
│   │   └── EftiMessageRepository.cs
│   ├── Interceptors/
│   │   └── AuditInterceptor.cs          ← salva audit_logs automaticamente
│   └── Migrations/                      ← generate da dotnet ef migrations add
│
└── DependencyInjection/
    └── InfrastructureExtensions.cs
```

```csharp
// DependencyInjection/InfrastructureExtensions.cs
namespace ilp_efti_connector.Infrastructure.DependencyInjection;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<AuditInterceptor>();

        services.AddDbContext<IlpEftiDbContext>((sp, options) =>
            options.UseMySql(
                configuration.GetConnectionString("DefaultConnection"),
                new MariaDbServerVersion(new Version(11, 4, 0)),
                mysql => mysql.EnableRetryOnFailure(3))
            .AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));

        // Registra repository
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerDestinationRepository, CustomerDestinationRepository>();
        services.AddScoped<ITransportOperationRepository, TransportOperationRepository>();
        services.AddScoped<IEftiMessageRepository, EftiMessageRepository>();

        return services;
    }
}
```

---

## 3. Progetti Shared

Contengono codice trasversale usato da più microservizi: eventi MassTransit, estensioni comuni, middleware HTTP.

### 3.1 — `EftiConnector.Shared.Contracts`

Definisce i messaggi del bus (eventi e comandi MassTransit) e i DTO di comunicazione inter-servizio. Non ha dipendenze verso infrastruttura.

```bash
dotnet new classlib -n ilp_efti_connector.Shared.Contracts \
  -o src/Shared/ilp_efti_connector.Shared.Contracts \
  -f net9.0

dotnet sln add src/Shared/ilp_efti_connector.Shared.Contracts/ilp_efti_connector.Shared.Contracts.csproj
```

**Package NuGet:**

```bash
cd src/Shared/ilp_efti_connector.Shared.Contracts
dotnet add package MassTransit --version 8.*
```

**Struttura interna:**

```
ilp_efti_connector.Shared.Contracts/
├── Events/
│   ├── TransportSubmittedEvent.cs
│   ├── TransportValidatedEvent.cs
│   ├── TransportValidationFailedEvent.cs
│   ├── EftiSendRequestedEvent.cs
│   ├── EftiResponseReceivedEvent.cs
│   └── SourceNotificationRequiredEvent.cs
├── Commands/
│   ├── SendToGatewayCommand.cs
│   └── RetryEftiMessageCommand.cs
└── Dtos/
    ├── SourcePayloadDto.cs
    └── EftiMessageStatusDto.cs
```

```csharp
// Events/TransportSubmittedEvent.cs
namespace ilp_efti_connector.Shared.Contracts.Events;

public record TransportSubmittedEvent(
    Guid   TransportOperationId,
    Guid   SourceId,
    string CorrelationId,
    string RawPayloadJson,
    string DatasetType,
    DateTime SubmittedAt
);
```

```csharp
// Events/EftiSendRequestedEvent.cs
namespace ilp_efti_connector.Shared.Contracts.Events;

public record EftiSendRequestedEvent(
    Guid   EftiMessageId,
    Guid   TransportOperationId,
    string CorrelationId,
    string GatewayProvider,   // "Milos" | "EftiNative"
    string PayloadJson,
    string DatasetType
);
```

---

### 3.2 — `EftiConnector.Shared.Infrastructure`

Contiene le estensioni di registrazione DI condivise tra microservizi: MassTransit, Redis, Serilog, health checks, middleware HTTP.

```bash
dotnet new classlib -n ilp_efti_connector.Shared.Infrastructure \
  -o src/Shared/ilp_efti_connector.Shared.Infrastructure \
  -f net9.0

dotnet sln add src/Shared/ilp_efti_connector.Shared.Infrastructure/ilp_efti_connector.Shared.Infrastructure.csproj

dotnet add src/Shared/ilp_efti_connector.Shared.Infrastructure/ilp_efti_connector.Shared.Infrastructure.csproj \
  reference src/Shared/ilp_efti_connector.Shared.Contracts/ilp_efti_connector.Shared.Contracts.csproj

dotnet add src/Shared/ilp_efti_connector.Shared.Infrastructure/ilp_efti_connector.Shared.Infrastructure.csproj \
  reference src/Core/ilp_efti_connector.Domain/ilp_efti_connector.Domain.csproj
```

**Package NuGet:**

```bash
cd src/Shared/ilp_efti_connector.Shared.Infrastructure

dotnet add package MassTransit.RabbitMQ --version 8.*
dotnet add package StackExchange.Redis --version 2.*
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis --version 9.*
dotnet add package Serilog.AspNetCore --version 8.*
dotnet add package Serilog.Sinks.Seq --version 5.*
dotnet add package Serilog.Enrichers.Environment --version 2.*
dotnet add package OpenTelemetry.Extensions.Hosting --version 1.*
dotnet add package OpenTelemetry.Instrumentation.AspNetCore --version 1.*
dotnet add package OpenTelemetry.Instrumentation.Http --version 1.*
dotnet add package OpenTelemetry.Instrumentation.EntityFrameworkCore --version 1.*
dotnet add package OpenTelemetry.Exporter.Prometheus.AspNetCore --version 1.*
dotnet add package AspNetCore.HealthChecks.MySql --version 8.*
dotnet add package AspNetCore.HealthChecks.RabbitMQ --version 8.*
dotnet add package AspNetCore.HealthChecks.Redis --version 8.*
dotnet add package AspNetCore.HealthChecks.Uris --version 8.*
dotnet add package HealthChecks.UI.Client --version 8.*
dotnet add package Polly --version 8.*
dotnet add package Microsoft.Extensions.Http.Polly --version 9.*
```

**Struttura interna:**

```
ilp_efti_connector.Shared.Infrastructure/
├── Extensions/
│   ├── MessagingExtensions.cs        ← AddIlpEftiMessaging(IServiceCollection)
│   ├── RedisExtensions.cs            ← AddIlpEftiRedis(...)
│   ├── LoggingExtensions.cs          ← UseSerilog(...)
│   ├── HealthCheckExtensions.cs      ← AddIlpEftiHealthChecks(...)
│   ├── TelemetryExtensions.cs        ← AddIlpEftiTelemetry(...)
│   └── AuthExtensions.cs             ← AddIlpEftiAuth(...)
├── Middleware/
│   ├── CorrelationIdMiddleware.cs
│   └── ExceptionHandlingMiddleware.cs
├── Resilience/
│   └── ResiliencePolicies.cs         ← Retry, CircuitBreaker, Timeout Polly
└── Metrics/
    └── IlpEftiMetrics.cs             ← Counter e Histogram Prometheus custom
```

---

## 4. Progetti Gateway (bifase)

Questi tre progetti implementano il pattern di astrazione per la bifase MILOS / EFTI Native.

### 4.1 — `EftiConnector.Gateway.Contracts`

Definisce `IEftiGateway` e i DTO condivisi tra le due implementazioni. **Nessuna dipendenza verso HTTP o librerie esterne.**

```bash
dotnet new classlib -n ilp_efti_connector.Gateway.Contracts \
  -o src/Gateway/ilp_efti_connector.Gateway.Contracts \
  -f net9.0

dotnet sln add src/Gateway/ilp_efti_connector.Gateway.Contracts/ilp_efti_connector.Gateway.Contracts.csproj

dotnet add src/Gateway/ilp_efti_connector.Gateway.Contracts/ilp_efti_connector.Gateway.Contracts.csproj \
  reference src/Core/ilp_efti_connector.Domain/ilp_efti_connector.Domain.csproj
```

**Struttura interna:**

```
ilp_efti_connector.Gateway.Contracts/
├── IEftiGateway.cs
├── Models/
│   ├── EcmrPayload.cs           ← modello interno normalizzato (input per entrambi i gateway)
│   ├── EftiSendResult.cs        ← risultato comune (output da entrambi i gateway)
│   └── GatewayHealthStatus.cs
└── Exceptions/
    ├── GatewayException.cs
    ├── GatewayAuthenticationException.cs
    └── GatewayTimeoutException.cs
```

```csharp
// IEftiGateway.cs
namespace ilp_efti_connector.Gateway.Contracts;

public interface IEftiGateway
{
    Task<EftiSendResult> SendEcmrAsync(EcmrPayload payload, CancellationToken ct = default);
    Task<EftiSendResult> UpdateEcmrAsync(string externalId, EcmrPayload payload, CancellationToken ct = default);
    Task<EftiSendResult> DeleteEcmrAsync(string externalId, CancellationToken ct = default);
    Task<EcmrPayload>    GetEcmrAsync(string externalId, CancellationToken ct = default);
    Task<bool>           HealthCheckAsync(CancellationToken ct = default);
}
```

```csharp
// Models/EcmrPayload.cs
namespace ilp_efti_connector.Gateway.Contracts.Models;

/// Modello interno normalizzato — indipendente dal provider.
/// Il mapper di ogni gateway lo converte nel formato specifico (MILOS o EN17532).
public record EcmrPayload(
    string  OperationCode,
    string  DatasetType,            // "eCMR" | "eDDT"
    bool    IsMasterCmr,
    string? Note,
    ConsignorInfo  Consignor,
    ConsigneeInfo  Consignee,
    IReadOnlyList<CarrierInfo> Carriers,
    AcceptanceLocation  PickupLocation,
    DeliveryLocation    DeliveryLocation,
    ConsignmentItems    Goods,
    TransportDetailsInfo TransportDetails
);

public record ConsignorInfo(
    string  Name,
    string? VatNumber,
    string? EoriCode,
    AddressInfo Address
);

public record AddressInfo(
    string  Street,
    string  City,
    string? PostalCode,
    string  CountryCode,        // ISO 3166-1 alpha-2
    string? CountryName,
    string? Province
);

public record CarrierInfo(
    string  Name,
    string? EoriCode,
    string? VatNumber,
    AddressInfo Address,
    string? TractorPlate,
    string? TrailerPlate,
    string? TractorPlateCountryCode,
    string? TrailerPlateCountryCode
);

public record EftiSendResult(
    bool    IsSuccess,
    string? ExternalId,         // eCMRID (MILOS) | messageId (EFTI)
    string? ExternalUuid,       // uuid da ECMRResponse MILOS; null in Fase 2
    string? ErrorCode,
    string? ErrorMessage
);
```

---

### 4.2 — `EftiConnector.Gateway.Milos` *(Fase 1)*

Implementa `IEftiGateway` chiamando le API MILOS TFP (Circle SpA).

```bash
dotnet new classlib -n ilp_efti_connector.Gateway.Milos \
  -o src/Gateway/ilp_efti_connector.Gateway.Milos \
  -f net9.0

dotnet sln add src/Gateway/ilp_efti_connector.Gateway.Milos/ilp_efti_connector.Gateway.Milos.csproj

dotnet add src/Gateway/ilp_efti_connector.Gateway.Milos/ilp_efti_connector.Gateway.Milos.csproj \
  reference src/Gateway/ilp_efti_connector.Gateway.Contracts/ilp_efti_connector.Gateway.Contracts.csproj

dotnet add src/Gateway/ilp_efti_connector.Gateway.Milos/ilp_efti_connector.Gateway.Milos.csproj \
  reference src/Shared/ilp_efti_connector.Shared.Infrastructure/ilp_efti_connector.Shared.Infrastructure.csproj
```

**Package NuGet:**

```bash
cd src/Gateway/ilp_efti_connector.Gateway.Milos

dotnet add package Refit --version 7.*
dotnet add package Refit.HttpClientFactory --version 7.*
dotnet add package Polly --version 8.*
dotnet add package Microsoft.Extensions.Http.Polly --version 9.*
dotnet add package Microsoft.Extensions.Options --version 9.*
```

**Struttura interna:**

```
ilp_efti_connector.Gateway.Milos/
├── MilosTfpGateway.cs                   ← implementa IEftiGateway
├── MilosGatewayOptions.cs               ← BaseUrl, ApiKey
│
├── Client/
│   ├── IMilosEcmrClient.cs              ← interfaccia Refit
│   └── MilosApiKeyHandler.cs            ← DelegatingHandler: inietta API Key header
│
├── Models/                              ← modelli ICD Circle SpA v1.0
│   ├── ECMRRequest.cs
│   ├── ECMRResponse.cs
│   ├── Shipping.cs
│   ├── Player.cs
│   ├── Carrier.cs
│   ├── PostalAddress.cs
│   ├── Signature.cs
│   ├── User.cs
│   ├── Document.cs
│   ├── ShippingMandate.cs
│   ├── ContractualCarrierAcceptanceLocation.cs
│   ├── ContractualConsigneeReceiptLocation.cs
│   ├── IncludedConsignmentItems.cs
│   ├── TransportPackage.cs
│   ├── TransportDetails.cs
│   ├── HashcodeDetails.cs
│   ├── OtherUsedTransportEquipment.cs
│   └── Enums/
│       ├── DatasetType.cs               // ECMR | EDDT
│       ├── PlayerType.cs
│       ├── CargoType.cs
│       ├── Incoterms.cs
│       └── EcmrEquipmentCategory.cs
│
├── Mapping/
│   └── EcmrPayloadToMilosMapper.cs      ← EcmrPayload → ECMRRequest
│
├── Hashing/
│   └── MilosHashcodeCalculator.cs       ← SHA-256 per hashcodeDetails
│
└── DependencyInjection/
    └── MilosGatewayExtensions.cs        ← AddMilosGateway(IServiceCollection)
```

```csharp
// Client/IMilosEcmrClient.cs
namespace ilp_efti_connector.Gateway.Milos.Client;

public interface IMilosEcmrClient
{
    [Post("/ecmr")]
    Task<ECMRResponse> CreateEcmrAsync([Body] ECMRRequest request, CancellationToken ct = default);

    [Put("/ecmr/{id}")]
    Task<string> UpdateEcmrAsync(string id, [Body] ECMRRequest request, CancellationToken ct = default);

    [Delete("/ecmr/{id}")]
    Task<string> DeleteEcmrAsync(string id, CancellationToken ct = default);

    [Get("/ecmr/get/{id}")]
    Task<ECMRRequest> GetEcmrAsync(string id, CancellationToken ct = default);
}
```

```csharp
// DependencyInjection/MilosGatewayExtensions.cs
namespace ilp_efti_connector.Gateway.Milos.DependencyInjection;

public static class MilosGatewayExtensions
{
    public static IServiceCollection AddMilosGateway(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MilosGatewayOptions>(
            configuration.GetSection("EftiGateway:Milos"));

        services.AddTransient<MilosApiKeyHandler>();

        services.AddRefitClient<IMilosEcmrClient>()
            .ConfigureHttpClient((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<MilosGatewayOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl);
                client.Timeout     = TimeSpan.FromSeconds(30);
            })
            .AddHttpMessageHandler<MilosApiKeyHandler>()
            .AddPolicyHandler(ResiliencePolicies.GetRetryPolicy())
            .AddPolicyHandler(ResiliencePolicies.GetCircuitBreakerPolicy());

        services.AddScoped<IEftiGateway, MilosTfpGateway>();

        return services;
    }
}
```

---

### 4.3 — `EftiConnector.Gateway.EftiNative` *(Fase 2)*

Implementa `IEftiGateway` comunicando direttamente con l'EFTI Gate nazionale.

```bash
dotnet new classlib -n ilp_efti_connector.Gateway.EftiNative \
  -o src/Gateway/ilp_efti_connector.Gateway.EftiNative \
  -f net9.0

dotnet sln add src/Gateway/ilp_efti_connector.Gateway.EftiNative/ilp_efti_connector.Gateway.EftiNative.csproj

dotnet add src/Gateway/ilp_efti_connector.Gateway.EftiNative/ilp_efti_connector.Gateway.EftiNative.csproj \
  reference src/Gateway/ilp_efti_connector.Gateway.Contracts/ilp_efti_connector.Gateway.Contracts.csproj

dotnet add src/Gateway/ilp_efti_connector.Gateway.EftiNative/ilp_efti_connector.Gateway.EftiNative.csproj \
  reference src/Shared/ilp_efti_connector.Shared.Infrastructure/ilp_efti_connector.Shared.Infrastructure.csproj
```

**Package NuGet:**

```bash
cd src/Gateway/ilp_efti_connector.Gateway.EftiNative

dotnet add package Refit --version 7.*
dotnet add package Refit.HttpClientFactory --version 7.*
dotnet add package Polly --version 8.*
dotnet add package Microsoft.Extensions.Http.Polly --version 9.*
dotnet add package Microsoft.IdentityModel.Tokens --version 8.*
dotnet add package System.IdentityModel.Tokens.Jwt --version 8.*
dotnet add package StackExchange.Redis --version 2.*
```

**Struttura interna:**

```
ilp_efti_connector.Gateway.EftiNative/
├── EftiNativeGateway.cs                 ← implementa IEftiGateway
├── EftiNativeOptions.cs                 ← BaseUrl, ClientId, CertPath, Scopes
│
├── Client/
│   ├── IEftiGateClient.cs               ← interfaccia Refit verso EFTI Gate
│   └── EftiOAuth2Handler.cs             ← DelegatingHandler: token OAuth2 + X.509
│
├── Auth/
│   ├── EftiTokenCache.cs                ← cache token Redis TTL 1h
│   └── X509CertificateLoader.cs         ← carica certificato da file o Key Vault
│
├── Models/
│   └── EN17532/                         ← dataset standard EFTI
│       ├── EftiEcmrDataset.cs
│       ├── EftiConsignor.cs
│       ├── EftiConsignee.cs
│       ├── EftiCarrier.cs
│       └── EftiGoods.cs
│
├── Mapping/
│   └── EcmrPayloadToEn17532Mapper.cs    ← EcmrPayload → EN 17532
│
├── AS4/                                 ← solo Fase 2, eDelivery
│   └── As4MessageBuilder.cs
│
└── DependencyInjection/
    └── EftiNativeGatewayExtensions.cs   ← AddEftiNativeGateway(IServiceCollection)
```

---

## 5. Microservizi (host)

Ogni microservizio è un progetto `webapi` o `worker` che **non contiene logica di business**: delega tutto ad `Application` e usa `IEftiGateway` tramite `Gateway.Contracts`. Di seguito le istruzioni per crearli tutti.

### 5.1 — `EftiConnector.ApiGateway`

```bash
dotnet new webapi -n ilp_efti_connectorApiGateway \
  -o src/Services/ilp_efti_connectorApiGateway \
  -f net9.0 --no-openapi

dotnet sln add src/Services/ilp_efti_connectorApiGateway/ilp_efti_connectorApiGateway.csproj

dotnet add src/Services/ilp_efti_connectorApiGateway/ilp_efti_connectorApiGateway.csproj \
  reference src/Shared/ilp_efti_connector.Shared.Infrastructure/ilp_efti_connector.Shared.Infrastructure.csproj

dotnet add src/Services/ilp_efti_connectorApiGateway/ilp_efti_connectorApiGateway.csproj \
  reference src/Shared/ilp_efti_connector.Shared.Contracts/ilp_efti_connector.Shared.Contracts.csproj

dotnet add src/Services/ilp_efti_connectorApiGateway/ilp_efti_connectorApiGateway.csproj \
  reference src/Core/ilp_efti_connector.Application/ilp_efti_connector.Application.csproj

dotnet add src/Services/ilp_efti_connectorApiGateway/ilp_efti_connectorApiGateway.csproj \
  reference src/Core/ilp_efti_connector.Infrastructure/ilp_efti_connector.Infrastructure.csproj
```

**Package NuGet:**

```bash
cd src/Services/ilp_efti_connectorApiGateway

dotnet add package Yarp.ReverseProxy --version 2.*
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 9.*
dotnet add package Scalar.AspNetCore --version 1.*
```

**Struttura interna:**

```
ilp_efti_connectorApiGateway/
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── Controllers/
│   ├── TransportController.cs       ← POST /api/v1/transport
│   └── HealthController.cs
├── Middleware/
│   └── SourceApiKeyMiddleware.cs    ← valida API Key sorgente
└── Extensions/
    └── ServiceCollectionExtensions.cs
```

---

### 5.2 — `EftiConnector.ValidationService`

```bash
dotnet new worker -n ilp_efti_connectorValidationService \
  -o src/Services/ilp_efti_connectorValidationService \
  -f net9.0

dotnet sln add src/Services/ilp_efti_connectorValidationService/ilp_efti_connectorValidationService.csproj

dotnet add src/Services/ilp_efti_connectorValidationService/ilp_efti_connectorValidationService.csproj \
  reference src/Shared/ilp_efti_connector.Shared.Infrastructure/ilp_efti_connector.Shared.Infrastructure.csproj

dotnet add src/Services/ilp_efti_connectorValidationService/ilp_efti_connectorValidationService.csproj \
  reference src/Shared/ilp_efti_connector.Shared.Contracts/ilp_efti_connector.Shared.Contracts.csproj
```

**Package NuGet:**

```bash
cd src/Services/ilp_efti_connectorValidationService
dotnet add package FluentValidation --version 11.*
dotnet add package MassTransit.RabbitMQ --version 8.*
```

**Struttura interna:**

```
ilp_efti_connectorValidationService/
├── Program.cs
├── appsettings.json
├── Consumers/
│   └── TransportSubmittedConsumer.cs    ← consuma TransportSubmittedEvent
├── Validators/
│   ├── MilosEcmrRequestValidator.cs     ← Fase 1: valida struttura MILOS
│   └── En17532DatasetValidator.cs       ← Fase 2: valida dataset EN 17532
└── Extensions/
    └── ServiceCollectionExtensions.cs
```

---

### 5.3 — `EftiConnector.NormalizationService`

```bash
dotnet new worker -n ilp_efti_connectorNormalizationService \
  -o src/Services/ilp_efti_connectorNormalizationService \
  -f net9.0

dotnet sln add src/Services/ilp_efti_connectorNormalizationService/ilp_efti_connectorNormalizationService.csproj

dotnet add src/Services/ilp_efti_connectorNormalizationService/ilp_efti_connectorNormalizationService.csproj \
  reference src/Core/ilp_efti_connector.Application/ilp_efti_connector.Application.csproj

dotnet add src/Services/ilp_efti_connectorNormalizationService/ilp_efti_connectorNormalizationService.csproj \
  reference src/Core/ilp_efti_connector.Infrastructure/ilp_efti_connector.Infrastructure.csproj

dotnet add src/Services/ilp_efti_connectorNormalizationService/ilp_efti_connectorNormalizationService.csproj \
  reference src/Shared/ilp_efti_connector.Shared.Infrastructure/ilp_efti_connector.Shared.Infrastructure.csproj

dotnet add src/Services/ilp_efti_connectorNormalizationService/ilp_efti_connectorNormalizationService.csproj \
  reference src/Shared/ilp_efti_connector.Shared.Contracts/ilp_efti_connector.Shared.Contracts.csproj

dotnet add src/Services/ilp_efti_connectorNormalizationService/ilp_efti_connectorNormalizationService.csproj \
  reference src/Gateway/ilp_efti_connector.Gateway.Contracts/ilp_efti_connector.Gateway.Contracts.csproj
```

**Struttura interna:**

```
ilp_efti_connectorNormalizationService/
├── Program.cs
├── appsettings.json
├── Consumers/
│   └── TransportValidatedConsumer.cs    ← consuma TransportValidatedEvent
├── Services/
│   └── CustomerUpsertService.cs         ← logica upsert cliente/destinazione
└── Mappers/
    └── SourcePayloadToEcmrMapper.cs     ← mappa payload sorgente → EcmrPayload
```

---

### 5.4 — `EftiConnector.EftiGatewayService` *(componente chiave bifase)*

```bash
dotnet new worker -n ilp_efti_connectorEftiGatewayService \
  -o src/Services/ilp_efti_connectorEftiGatewayService \
  -f net9.0

dotnet sln add src/Services/ilp_efti_connectorEftiGatewayService/ilp_efti_connectorEftiGatewayService.csproj

dotnet add src/Services/ilp_efti_connectorEftiGatewayService/ilp_efti_connectorEftiGatewayService.csproj \
  reference src/Shared/ilp_efti_connector.Shared.Infrastructure/ilp_efti_connector.Shared.Infrastructure.csproj

dotnet add src/Services/ilp_efti_connectorEftiGatewayService/ilp_efti_connectorEftiGatewayService.csproj \
  reference src/Shared/ilp_efti_connector.Shared.Contracts/ilp_efti_connector.Shared.Contracts.csproj

dotnet add src/Services/ilp_efti_connectorEftiGatewayService/ilp_efti_connectorEftiGatewayService.csproj \
  reference src/Core/ilp_efti_connector.Infrastructure/ilp_efti_connector.Infrastructure.csproj

# Dipende da Gateway.Contracts (interfaccia) + entrambe le implementazioni
dotnet add src/Services/ilp_efti_connectorEftiGatewayService/ilp_efti_connectorEftiGatewayService.csproj \
  reference src/Gateway/ilp_efti_connector.Gateway.Contracts/ilp_efti_connector.Gateway.Contracts.csproj

dotnet add src/Services/ilp_efti_connectorEftiGatewayService/ilp_efti_connectorEftiGatewayService.csproj \
  reference src/Gateway/ilp_efti_connector.Gateway.Milos/ilp_efti_connector.Gateway.Milos.csproj

dotnet add src/Services/ilp_efti_connectorEftiGatewayService/ilp_efti_connectorEftiGatewayService.csproj \
  reference src/Gateway/ilp_efti_connector.Gateway.EftiNative/ilp_efti_connector.Gateway.EftiNative.csproj
```

**Struttura interna:**

```
ilp_efti_connectorEftiGatewayService/
├── Program.cs                           ← switch provider MILOS/EFTI via config
├── appsettings.json
├── Consumers/
│   └── EftiSendConsumer.cs             ← usa IEftiGateway — identico in Fase 1 e Fase 2
└── Extensions/e mde
    └── ServiceCollectionExtensions.cs
```

```csharp
// Program.cs — selezione provider a runtime
var provider = builder.Configuration["EftiGateway:Provider"]
    ?? throw new InvalidOperationException("EftiGateway:Provider non configurato.");

switch (provider)
{
    case "Milos":
        builder.Services.AddMilosGateway(builder.Configuration);
        break;
    case "EftiNative":
        builder.Services.AddEftiNativeGateway(builder.Configuration);
        break;
    default:
        throw new InvalidOperationException($"Provider non riconosciuto: {provider}");
}
```

---

### 5.5 — `EftiConnector.ResponseHandlerService`

```bash
dotnet new worker -n ilp_efti_connectorResponseHandlerService \
  -o src/Services/ilp_efti_connectorResponseHandlerService \
  -f net9.0

dotnet sln add src/Services/ilp_efti_connectorResponseHandlerService/ilp_efti_connectorResponseHandlerService.csproj

dotnet add src/Services/ilp_efti_connectorResponseHandlerService/ilp_efti_connectorResponseHandlerService.csproj \
  reference src/Core/ilp_efti_connector.Infrastructure/ilp_efti_connector.Infrastructure.csproj

dotnet add src/Services/ilp_efti_connectorResponseHandlerService/ilp_efti_connectorResponseHandlerService.csproj \
  reference src/Shared/ilp_efti_connector.Shared.Infrastructure/ilp_efti_connector.Shared.Infrastructure.csproj

dotnet add src/Services/ilp_efti_connectorResponseHandlerService/ilp_efti_connectorResponseHandlerService.csproj \
  reference src/Shared/ilp_efti_connector.Shared.Contracts/ilp_efti_connector.Shared.Contracts.csproj
```

**Struttura interna:**

```
ilp_efti_connectorResponseHandlerService/
├── Program.cs
├── appsettings.json
└── Consumers/
    └── EftiResponseReceivedConsumer.cs  ← aggiorna status, pubblica notifica
```

---

### 5.6 — `EftiConnector.NotificationService`

```bash
dotnet new worker -n ilp_efti_connectorNotificationService \
  -o src/Services/ilp_efti_connectorNotificationService \
  -f net9.0

dotnet sln add src/Services/ilp_efti_connectorNotificationService/ilp_efti_connectorNotificationService.csproj

dotnet add src/Services/ilp_efti_connectorNotificationService/ilp_efti_connectorNotificationService.csproj \
  reference src/Core/ilp_efti_connector.Infrastructure/ilp_efti_connector.Infrastructure.csproj

dotnet add src/Services/ilp_efti_connectorNotificationService/ilp_efti_connectorNotificationService.csproj \
  reference src/Shared/ilp_efti_connector.Shared.Infrastructure/ilp_efti_connector.Shared.Infrastructure.csproj

dotnet add src/Services/ilp_efti_connectorNotificationService/ilp_efti_connectorNotificationService.csproj \
  reference src/Shared/ilp_efti_connector.Shared.Contracts/ilp_efti_connector.Shared.Contracts.csproj
```

**Package NuGet:**

```bash
cd src/Services/ilp_efti_connectorNotificationService
dotnet add package Hangfire.Core --version 1.*
dotnet add package Hangfire.MySqlStorage --version 2.*
dotnet add package Hangfire.AspNetCore --version 1.*
```

**Struttura interna:**

```
ilp_efti_connectorNotificationService/
├── Program.cs
├── appsettings.json
├── Consumers/
│   └── SourceNotificationConsumer.cs    ← consuma SourceNotificationRequiredEvent
├── Services/
│   ├── WebhookSenderService.cs          ← HTTP POST verso endpoint sorgente
│   └── SseHubService.cs                 ← Server-Sent Events per React UI
└── Jobs/
    └── WebhookRetryJob.cs               ← Hangfire job con backoff
```

---

### 5.7 — `EftiConnector.RetryService`

```bash
dotnet new worker -n ilp_efti_connectorRetryService \
  -o src/Services/ilp_efti_connectorRetryService \
  -f net9.0

dotnet sln add src/Services/ilp_efti_connectorRetryService/ilp_efti_connectorRetryService.csproj

dotnet add src/Services/ilp_efti_connectorRetryService/ilp_efti_connectorRetryService.csproj \
  reference src/Core/ilp_efti_connector.Infrastructure/ilp_efti_connector.Infrastructure.csproj

dotnet add src/Services/ilp_efti_connectorRetryService/ilp_efti_connectorRetryService.csproj \
  reference src/Shared/ilp_efti_connector.Shared.Infrastructure/ilp_efti_connector.Shared.Infrastructure.csproj

dotnet add src/Services/ilp_efti_connectorRetryService/ilp_efti_connectorRetryService.csproj \
  reference src/Shared/ilp_efti_connector.Shared.Contracts/ilp_efti_connector.Shared.Contracts.csproj
```

**Struttura interna:**

```
ilp_efti_connectorRetryService/
├── Program.cs
├── appsettings.json
└── Workers/
    ├── RetryWorker.cs                   ← polling DB per status RETRY, backoff
    └── DeadLetterWorker.cs              ← processa DLQ RabbitMQ, alert dopo 6 tentativi
```

---

### 5.8 — `EftiConnector.QueryProxyService`

```bash
dotnet new webapi -n ilp_efti_connectorQueryProxyService \
  -o src/Services/ilp_efti_connectorQueryProxyService \
  -f net9.0 --no-openapi

dotnet sln add src/Services/ilp_efti_connectorQueryProxyService/ilp_efti_connectorQueryProxyService.csproj

dotnet add src/Services/ilp_efti_connectorQueryProxyService/ilp_efti_connectorQueryProxyService.csproj \
  reference src/Core/ilp_efti_connector.Infrastructure/ilp_efti_connector.Infrastructure.csproj

dotnet add src/Services/ilp_efti_connectorQueryProxyService/ilp_efti_connectorQueryProxyService.csproj \
  reference src/Shared/ilp_efti_connector.Shared.Infrastructure/ilp_efti_connector.Shared.Infrastructure.csproj

# Solo in Fase 2 — attivo ma restituisce 501 in Fase 1
dotnet add src/Services/ilp_efti_connectorQueryProxyService/ilp_efti_connectorQueryProxyService.csproj \
  reference src/Gateway/ilp_efti_connector.Gateway.Contracts/ilp_efti_connector.Gateway.Contracts.csproj
```

**Struttura interna:**

```
ilp_efti_connectorQueryProxyService/
├── Program.cs
├── appsettings.json
└── Controllers/
    └── QueryController.cs               ← GET /api/v1/queries/{id}
                                         ← ritorna 501 in Fase 1, attivo in Fase 2
```

---

### 5.9 — `EftiConnector.FormInputService`

```bash
dotnet new webapi -n ilp_efti_connectorFormInputService \
  -o src/Services/ilp_efti_connectorFormInputService \
  -f net9.0 --no-openapi

dotnet sln add src/Services/ilp_efti_connectorFormInputService/ilp_efti_connectorFormInputService.csproj

dotnet add src/Services/ilp_efti_connectorFormInputService/ilp_efti_connectorFormInputService.csproj \
  reference src/Core/ilp_efti_connector.Application/ilp_efti_connector.Application.csproj

dotnet add src/Services/ilp_efti_connectorFormInputService/ilp_efti_connectorFormInputService.csproj \
  reference src/Core/ilp_efti_connector.Infrastructure/ilp_efti_connector.Infrastructure.csproj

dotnet add src/Services/ilp_efti_connectorFormInputService/ilp_efti_connectorFormInputService.csproj \
  reference src/Shared/ilp_efti_connector.Shared.Infrastructure/ilp_efti_connector.Shared.Infrastructure.csproj

dotnet add src/Services/ilp_efti_connectorFormInputService/ilp_efti_connectorFormInputService.csproj \
  reference src/Shared/ilp_efti_connector.Shared.Contracts/ilp_efti_connector.Shared.Contracts.csproj
```

**Package NuGet:**

```bash
cd src/Services/ilp_efti_connectorFormInputService
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 9.*
dotnet add package Scalar.AspNetCore --version 1.*
```

**Struttura interna:**

```
ilp_efti_connectorFormInputService/
├── Program.cs
├── appsettings.json
└── Controllers/
    ├── TransportOperationsController.cs ← CRUD + draft autosave
    ├── CustomersController.cs           ← anagrafica clienti
    ├── CustomerDestinationsController.cs
    ├── SourcesController.cs             ← solo admin
    ├── EftiMessagesController.cs        ← elenco messaggi + dettaglio
    ├── AuditController.cs               ← log audit
    └── AdminController.cs               ← switch provider, utenti, parametri
```

---

## 6. Progetti di Test

### 6.1 — `EftiConnector.Domain.Tests`

```bash
dotnet new xunit -n ilp_efti_connector.Domain.Tests \
  -o tests/ilp_efti_connector.Domain.Tests \
  -f net9.0

dotnet sln add tests/ilp_efti_connector.Domain.Tests/ilp_efti_connector.Domain.Tests.csproj

dotnet add tests/ilp_efti_connector.Domain.Tests/ilp_efti_connector.Domain.Tests.csproj \
  reference src/Core/ilp_efti_connector.Domain/ilp_efti_connector.Domain.csproj
```

**Package NuGet:**

```bash
cd tests/ilp_efti_connector.Domain.Tests
dotnet add package FluentAssertions --version 6.*
```

---

### 6.2 — `EftiConnector.Application.Tests`

```bash
dotnet new xunit -n ilp_efti_connector.Application.Tests \
  -o tests/ilp_efti_connector.Application.Tests \
  -f net9.0

dotnet sln add tests/ilp_efti_connector.Application.Tests/ilp_efti_connector.Application.Tests.csproj

dotnet add tests/ilp_efti_connector.Application.Tests/ilp_efti_connector.Application.Tests.csproj \
  reference src/Core/ilp_efti_connector.Application/ilp_efti_connector.Application.csproj

dotnet add tests/ilp_efti_connector.Application.Tests/ilp_efti_connector.Application.Tests.csproj \
  reference src/Core/ilp_efti_connector.Domain/ilp_efti_connector.Domain.csproj
```

**Package NuGet:**

```bash
cd tests/ilp_efti_connector.Application.Tests
dotnet add package Moq --version 4.*
dotnet add package FluentAssertions --version 6.*
dotnet add package MediatR --version 12.*
```

---

### 6.3 — `EftiConnector.Gateway.Milos.Tests`

```bash
dotnet new xunit -n ilp_efti_connector.Gateway.Milos.Tests \
  -o tests/ilp_efti_connector.Gateway.Milos.Tests \
  -f net9.0

dotnet sln add tests/ilp_efti_connector.Gateway.Milos.Tests/ilp_efti_connector.Gateway.Milos.Tests.csproj

dotnet add tests/ilp_efti_connector.Gateway.Milos.Tests/ilp_efti_connector.Gateway.Milos.Tests.csproj \
  reference src/Gateway/ilp_efti_connector.Gateway.Milos/ilp_efti_connector.Gateway.Milos.csproj

dotnet add tests/ilp_efti_connector.Gateway.Milos.Tests/ilp_efti_connector.Gateway.Milos.Tests.csproj \
  reference src/Gateway/ilp_efti_connector.Gateway.Contracts/ilp_efti_connector.Gateway.Contracts.csproj
```

**Package NuGet:**

```bash
cd tests/ilp_efti_connector.Gateway.Milos.Tests
dotnet add package Moq --version 4.*
dotnet add package FluentAssertions --version 6.*
dotnet add package WireMock.Net --version 1.*           # mock server HTTP per test Refit
dotnet add package Microsoft.Extensions.Http --version 9.*
```

---

### 6.4 — `EftiConnector.Gateway.EftiNative.Tests`

```bash
dotnet new xunit -n ilp_efti_connector.Gateway.EftiNative.Tests \
  -o tests/ilp_efti_connector.Gateway.EftiNative.Tests \
  -f net9.0

dotnet sln add tests/ilp_efti_connector.Gateway.EftiNative.Tests/ilp_efti_connector.Gateway.EftiNative.Tests.csproj

dotnet add tests/ilp_efti_connector.Gateway.EftiNative.Tests/ilp_efti_connector.Gateway.EftiNative.Tests.csproj \
  reference src/Gateway/ilp_efti_connector.Gateway.EftiNative/ilp_efti_connector.Gateway.EftiNative.csproj

dotnet add tests/ilp_efti_connector.Gateway.EftiNative.Tests/ilp_efti_connector.Gateway.EftiNative.Tests.csproj \
  reference src/Gateway/ilp_efti_connector.Gateway.Contracts/ilp_efti_connector.Gateway.Contracts.csproj
```

**Package NuGet:**

```bash
cd tests/ilp_efti_connector.Gateway.EftiNative.Tests
dotnet add package Moq --version 4.*
dotnet add package FluentAssertions --version 6.*
dotnet add package WireMock.Net --version 1.*
```

---

### 6.5 — `EftiConnector.IntegrationTests`

```bash
dotnet new xunit -n ilp_efti_connectorIntegrationTests \
  -o tests/ilp_efti_connectorIntegrationTests \
  -f net9.0

dotnet sln add tests/ilp_efti_connectorIntegrationTests/ilp_efti_connectorIntegrationTests.csproj

# Referenzia i microservizi da testare end-to-end
dotnet add tests/ilp_efti_connectorIntegrationTests/ilp_efti_connectorIntegrationTests.csproj \
  reference src/Services/ilp_efti_connectorApiGateway/ilp_efti_connectorApiGateway.csproj

dotnet add tests/ilp_efti_connectorIntegrationTests/ilp_efti_connectorIntegrationTests.csproj \
  reference src/Core/ilp_efti_connector.Infrastructure/ilp_efti_connector.Infrastructure.csproj
```

**Package NuGet:**

```bash
cd tests/ilp_efti_connectorIntegrationTests
dotnet add package Testcontainers --version 3.*
dotnet add package Testcontainers.MariaDb --version 3.*
dotnet add package Testcontainers.RabbitMq --version 3.*
dotnet add package Testcontainers.Redis --version 3.*
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 9.*
dotnet add package FluentAssertions --version 6.*
dotnet add package WireMock.Net --version 1.*
```

---

## 7. Configurazione Package References

### `Directory.Build.props` — nella root della solution

Questo file centralizza le versioni dei package e le proprietà comuni a tutti i progetti, eliminando duplicazioni nei singoli `.csproj`.

```xml
<!-- Directory.Build.props — root della solution -->
<Project>
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <AnalysisLevel>latest</AnalysisLevel>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>
</Project>
```

### `Directory.Packages.props` — nella root della solution

Central Package Management (CPM): le versioni sono definite qui una sola volta.

```xml
<!-- Directory.Packages.props — root della solution -->
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
    <!-- Entity Framework Core + MariaDB -->
    <PackageVersion Include="Microsoft.EntityFrameworkCore"              Version="9.0.1" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Design"       Version="9.0.1" />
    <PackageVersion Include="Pomelo.EntityFrameworkCore.MySql"           Version="9.0.0" />

    <!-- MassTransit -->
    <PackageVersion Include="MassTransit"                                Version="8.2.5" />
    <PackageVersion Include="MassTransit.RabbitMQ"                       Version="8.2.5" />

    <!-- Refit + Polly -->
    <PackageVersion Include="Refit"                                      Version="7.1.2" />
    <PackageVersion Include="Refit.HttpClientFactory"                    Version="7.1.2" />
    <PackageVersion Include="Polly"                                      Version="8.3.1" />
    <PackageVersion Include="Microsoft.Extensions.Http.Polly"           Version="9.0.1" />

    <!-- Redis -->
    <PackageVersion Include="StackExchange.Redis"                        Version="2.7.33" />
    <PackageVersion Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="9.0.1" />

    <!-- Auth -->
    <PackageVersion Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.1" />

    <!-- Logging -->
    <PackageVersion Include="Serilog.AspNetCore"                         Version="8.0.2" />
    <PackageVersion Include="Serilog.Sinks.Seq"                         Version="8.0.0" />
    <PackageVersion Include="Serilog.Enrichers.Environment"             Version="3.0.0" />

    <!-- Observability -->
    <PackageVersion Include="OpenTelemetry.Extensions.Hosting"          Version="1.9.0" />
    <PackageVersion Include="OpenTelemetry.Instrumentation.AspNetCore"  Version="1.9.0" />
    <PackageVersion Include="OpenTelemetry.Instrumentation.Http"        Version="1.9.0" />
    <PackageVersion Include="OpenTelemetry.Instrumentation.EntityFrameworkCore" Version="1.0.0-beta.12" />
    <PackageVersion Include="OpenTelemetry.Exporter.Prometheus.AspNetCore" Version="1.9.0" />

    <!-- Health Checks -->
    <PackageVersion Include="AspNetCore.HealthChecks.MySql"             Version="8.0.2" />
    <PackageVersion Include="AspNetCore.HealthChecks.RabbitMQ"          Version="8.0.2" />
    <PackageVersion Include="AspNetCore.HealthChecks.Redis"             Version="8.0.2" />
    <PackageVersion Include="AspNetCore.HealthChecks.Uris"              Version="8.0.2" />
    <PackageVersion Include="HealthChecks.UI.Client"                    Version="8.0.2" />

    <!-- Validation + Mapping -->
    <PackageVersion Include="FluentValidation"                           Version="11.9.2" />
    <PackageVersion Include="FluentValidation.DependencyInjectionExtensions" Version="11.9.2" />
    <PackageVersion Include="MediatR"                                    Version="12.2.0" />
    <PackageVersion Include="Mapster"                                    Version="7.4.0" />
    <PackageVersion Include="Mapster.DependencyInjection"               Version="1.0.2" />

    <!-- Background Jobs -->
    <PackageVersion Include="Hangfire.Core"                             Version="1.8.14" />
    <PackageVersion Include="Hangfire.MySqlStorage"                     Version="2.0.3" />
    <PackageVersion Include="Hangfire.AspNetCore"                       Version="1.8.14" />

    <!-- API Gateway -->
    <PackageVersion Include="Yarp.ReverseProxy"                         Version="2.1.0" />
    <PackageVersion Include="Scalar.AspNetCore"                         Version="1.3.8" />

    <!-- Test -->
    <PackageVersion Include="xunit"                                     Version="2.9.0" />
    <PackageVersion Include="xunit.runner.visualstudio"                 Version="2.8.2" />
    <PackageVersion Include="Moq"                                       Version="4.20.72" />
    <PackageVersion Include="FluentAssertions"                          Version="6.12.1" />
    <PackageVersion Include="Testcontainers"                            Version="3.9.0" />
    <PackageVersion Include="Testcontainers.MariaDb"                    Version="3.9.0" />
    <PackageVersion Include="Testcontainers.RabbitMq"                   Version="3.9.0" />
    <PackageVersion Include="Testcontainers.Redis"                      Version="3.9.0" />
    <PackageVersion Include="Microsoft.AspNetCore.Mvc.Testing"          Version="9.0.1" />
    <PackageVersion Include="WireMock.Net"                              Version="1.6.5" />
  </ItemGroup>
</Project>
```

Una volta impostato CPM, i singoli `.csproj` referenziano i package **senza specificare la versione**:

```xml
<!-- esempio in ilp_efti_connector.Gateway.Milos.csproj -->
<ItemGroup>
  <PackageReference Include="Refit" />
  <PackageReference Include="Refit.HttpClientFactory" />
  <PackageReference Include="Polly" />
</ItemGroup>
```

---

## 8. Struttura interna di ogni progetto

### Convenzioni obbligatorie

**Namespace:** ogni progetto usa il proprio namespace radice corrispondente al nome del progetto.

```csharp
// ✅ Corretto
namespace ilp_efti_connector.Domain.Entities;
namespace ilp_efti_connector.Gateway.Milos.Client;

// ❌ Scorretto
namespace ilp_efti_connectorEntities;
namespace Milos.Client;
```

**`Program.cs` di ogni microservizio** segue sempre questo schema:

```csharp
var builder = WebApplication.CreateBuilder(args); // o Host.CreateApplicationBuilder

// 1. Logging
builder.Host.UseSerilog(...);

// 2. Servizi shared
builder.Services.AddIlpEftiMessaging(builder.Configuration);
builder.Services.AddIlpEftiRedis(builder.Configuration);
builder.Services.AddIlpEftiAuth(builder.Configuration);
builder.Services.AddIlpEftiHealthChecks(builder.Configuration);
builder.Services.AddIlpEftiTelemetry(builder.Configuration);

// 3. Servizi specifici del microservizio
builder.Services.AddInfrastructure(builder.Configuration);  // se usa DB
// ... consumer, validator, mapper del microservizio ...

// 4. Gateway (solo EftiGatewayService)
// builder.Services.AddMilosGateway(...) o AddEftiNativeGateway(...)

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapHealthChecks("/health/live",  ...);
app.MapHealthChecks("/health/ready", ...);
app.MapPrometheusScrapingEndpoint("/metrics");

app.Run();
```

### `.editorconfig` — nella root della solution

```ini
root = true

[*.cs]
indent_style = space
indent_size = 4
end_of_line = lf
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = true

# Naming conventions
dotnet_naming_rule.interface_should_start_with_i.symbols = interface
dotnet_naming_rule.interface_should_start_with_i.style   = begins_with_i
dotnet_naming_symbols.interface.applicable_kinds = interface
dotnet_naming_style.begins_with_i.required_prefix = I

# var preferences
csharp_style_var_for_built_in_types    = false:suggestion
csharp_style_var_when_type_is_apparent = true:suggestion
csharp_style_var_elsewhere             = false:suggestion

# Expression body
csharp_style_expression_bodied_methods     = false:silent
csharp_style_expression_bodied_properties  = true:suggestion

# Null checking
csharp_style_throw_expression              = true:suggestion
dotnet_style_null_propagation              = true:suggestion
dotnet_style_coalesce_expression           = true:suggestion
```

### `.gitignore` — nella root della solution

```
# Build output
**/bin/
**/obj/
**/publish/

# User-specific
*.user
.vs/
.idea/
*.suo

# Secrets — MAI committare
infra/docker/.env
**/appsettings.*.json
!**/appsettings.json
!**/appsettings.Development.json
*.pfx
*.p12
certs/

# Test results
**/TestResults/
**/coverage/

# Node / frontend
frontend/**/node_modules/
frontend/**/.vite/
```

---

## 9. Verifica finale della solution

Dopo aver creato tutti i progetti, esegui questi comandi per verificare che tutto sia corretto.

```bash
# Dalla root della solution

# 1. Verifica che tutti i progetti siano nell'sln
dotnet sln list

# Output atteso (18 progetti):
# src/Core/ilp_efti_connector.Domain/ilp_efti_connector.Domain.csproj
# src/Core/ilp_efti_connector.Application/ilp_efti_connector.Application.csproj
# src/Core/ilp_efti_connector.Infrastructure/ilp_efti_connector.Infrastructure.csproj
# src/Shared/ilp_efti_connector.Shared.Contracts/ilp_efti_connector.Shared.Contracts.csproj
# src/Shared/ilp_efti_connector.Shared.Infrastructure/ilp_efti_connector.Shared.Infrastructure.csproj
# src/Gateway/ilp_efti_connector.Gateway.Contracts/ilp_efti_connector.Gateway.Contracts.csproj
# src/Gateway/ilp_efti_connector.Gateway.Milos/ilp_efti_connector.Gateway.Milos.csproj
# src/Gateway/ilp_efti_connector.Gateway.EftiNative/ilp_efti_connector.Gateway.EftiNative.csproj
# src/Services/ilp_efti_connectorApiGateway/ilp_efti_connectorApiGateway.csproj
# src/Services/ilp_efti_connectorValidationService/ilp_efti_connectorValidationService.csproj
# src/Services/ilp_efti_connectorNormalizationService/ilp_efti_connectorNormalizationService.csproj
# src/Services/ilp_efti_connectorEftiGatewayService/ilp_efti_connectorEftiGatewayService.csproj
# src/Services/ilp_efti_connectorResponseHandlerService/ilp_efti_connectorResponseHandlerService.csproj
# src/Services/ilp_efti_connectorNotificationService/ilp_efti_connectorNotificationService.csproj
# src/Services/ilp_efti_connectorRetryService/ilp_efti_connectorRetryService.csproj
# src/Services/ilp_efti_connectorQueryProxyService/ilp_efti_connectorQueryProxyService.csproj
# src/Services/ilp_efti_connectorFormInputService/ilp_efti_connectorFormInputService.csproj
# tests/ilp_efti_connector.Domain.Tests/ilp_efti_connector.Domain.Tests.csproj
# tests/ilp_efti_connector.Application.Tests/ilp_efti_connector.Application.Tests.csproj
# tests/ilp_efti_connector.Gateway.Milos.Tests/ilp_efti_connector.Gateway.Milos.Tests.csproj
# tests/ilp_efti_connector.Gateway.EftiNative.Tests/ilp_efti_connector.Gateway.EftiNative.Tests.csproj
# tests/ilp_efti_connectorIntegrationTests/ilp_efti_connectorIntegrationTests.csproj

# 2. Build completa (deve passare senza errori)
dotnet build --configuration Release

# 3. Run test unitari
dotnet test tests/ilp_efti_connector.Domain.Tests
dotnet test tests/ilp_efti_connector.Application.Tests

# 4. Controlla i riferimenti circolari
dotnet build 2>&1 | grep -i "circular"
# → nessun output = nessun riferimento circolare

# 5. Verifica Central Package Management
dotnet restore 2>&1 | grep -i "error\|warning"
# → solo warning accettabili di framework, nessun errore di versione
```

### Matrice dipendenze — riepilogo

```
                   Domain  Application  Infrastructure  Shared.Contracts  Shared.Infrastructure  Gateway.Contracts  Gateway.Milos  Gateway.EftiNative
Domain                –
Application           ✓        –
Infrastructure        ✓        ✓              –
Shared.Contracts                               –
Shared.Infrastructure ✓                        ✓               –
Gateway.Contracts     ✓                                                            –
Gateway.Milos                                              ✓                       ✓                 –
Gateway.EftiNative                                         ✓                       ✓                                    –
ApiGateway                     ✓              ✓            ✓                ✓
ValidationService                                           ✓                ✓
NormalizationService           ✓              ✓            ✓                ✓                ✓
EftiGatewayService                            ✓            ✓                ✓                ✓            ✓              ✓
ResponseHandlerService                         ✓            ✓                ✓
NotificationService                            ✓            ✓                ✓
RetryService                                   ✓            ✓                ✓
QueryProxyService                              ✓                             ✓                ✓
FormInputService               ✓              ✓            ✓                ✓
```

---

*EFTI Connector Platform — Solution Setup Guide v1.0 — Febbraio 2026*

using ilp_efti_connector.Application.Common.Interfaces;
using ilp_efti_connector.Infrastructure.DependencyInjection;
using ilp_efti_connector.Infrastructure.Persistence;
using ilp_efti_connector.IntegrationTests.Infrastructure;
using ilp_efti_connector.NormalizationService.Consumers;
using ilp_efti_connector.Shared.Contracts.Events;
using ilp_efti_connector.ValidationService.Consumers;
using MassTransit;
using MassTransit.Testing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ilp_efti_connector.IntegrationTests.Pipeline;

/// <summary>
/// Test end-to-end del flusso: TransportSubmittedEvent → ValidationService →
/// TransportValidatedEvent → NormalizationService → persistenza DB → EftiSendRequestedEvent.
/// Usa MariaDB Testcontainers (DB reale) e MassTransit InMemory transport.
/// </summary>
[Collection(MariaDbCollection.CollectionName)]
public sealed class FullPipelineTests : IAsyncLifetime
{
    private readonly MariaDbContainerFixture _dbFixture;
    private ServiceProvider _provider = null!;
    private ITestHarness    _harness  = null!;

    private static readonly Guid SeededSourceId = new("11111111-1111-1111-1111-111111111111");

    public FullPipelineTests(MariaDbContainerFixture dbFixture)
        => _dbFixture = dbFixture;

    public async Task InitializeAsync()
    {
        _provider = new ServiceCollection()
            .AddLogging(b => b.SetMinimumLevel(LogLevel.Warning))
            .AddDbContext<EftiConnectorDbContext>(options =>
                options.UseMySql(
                    _dbFixture.ConnectionString,
                    new MariaDbServerVersion(new Version(11, 4, 0))))
            .AddScoped<ilp_efti_connector.Domain.Interfaces.Repositories.ISourceRepository,
                       ilp_efti_connector.Infrastructure.Persistence.Repositories.SourceRepository>()
            .AddScoped<ilp_efti_connector.Domain.Interfaces.Repositories.ICustomerRepository,
                       ilp_efti_connector.Infrastructure.Persistence.Repositories.CustomerRepository>()
            .AddScoped<ilp_efti_connector.Domain.Interfaces.Repositories.ICustomerDestinationRepository,
                       ilp_efti_connector.Infrastructure.Persistence.Repositories.CustomerDestinationRepository>()
            .AddScoped<ilp_efti_connector.Domain.Interfaces.Repositories.ITransportOperationRepository,
                       ilp_efti_connector.Infrastructure.Persistence.Repositories.TransportOperationRepository>()
            .AddScoped<ilp_efti_connector.Domain.Interfaces.Repositories.IEftiMessageRepository,
                       ilp_efti_connector.Infrastructure.Persistence.Repositories.EftiMessageRepository>()
            .AddScoped<ilp_efti_connector.Domain.Interfaces.Repositories.IAuditLogRepository,
                       ilp_efti_connector.Infrastructure.Persistence.Repositories.AuditLogRepository>()
            .AddScoped<ilp_efti_connector.Domain.Interfaces.Repositories.IUnitOfWork,
                       ilp_efti_connector.Infrastructure.Persistence.UnitOfWork>()
            .AddSingleton<ICurrentUserService, NullCurrentUserService>()
            .AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(
                    typeof(ilp_efti_connector.Application.Customers.Commands.UpsertCustomer
                        .UpsertCustomerCommand).Assembly))
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<TransportSubmittedConsumer>();
                cfg.AddConsumer<TransportValidatedConsumer>();
            })
            .BuildServiceProvider(true);

        _harness = _provider.GetRequiredService<ITestHarness>();
        await _harness.Start();
    }

    public async Task DisposeAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

    // ── Test pipeline completa ────────────────────────────────────────────────

    [Fact]
    public async Task FullPipeline_ValidEvent_ShouldPublish_EftiSendRequestedEvent()
    {
        var operationCode = $"PIPE-{Guid.NewGuid():N}";
        var evt           = BuildEvent(operationCode);

        await _harness.Bus.Publish(evt);

        (await _harness.Published.Any<EftiSendRequestedEvent>(
            msg => msg.Context.Message.CorrelationId == evt.CorrelationId,
            CancellationToken.None))
            .Should().BeTrue("il flusso completo deve produrre un EftiSendRequestedEvent");
    }

    [Fact]
    public async Task FullPipeline_ValidEvent_ShouldPersist_TransportOperation_InDb()
    {
        var operationCode = $"DBPIPE-{Guid.NewGuid():N}";
        var evt           = BuildEvent(operationCode);

        await _harness.Bus.Publish(evt);

        // attende EftiSendRequestedEvent per assicurarsi che la normalizzazione sia completata
        await _harness.Published.Any<EftiSendRequestedEvent>(
            msg => msg.Context.Message.CorrelationId == evt.CorrelationId,
            CancellationToken.None);

        await using var scope = _provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EftiConnectorDbContext>();

        var operation = await db.TransportOperations
            .Include(o => o.EftiMessages)
            .FirstOrDefaultAsync(o => o.OperationCode == operationCode);

        operation.Should().NotBeNull("la TransportOperation deve essere persistita nel DB");
        operation!.EftiMessages.Should().ContainSingle("deve esserci un EftiMessage iniziale");
        operation.EftiMessages.First().GatewayProvider.Should().Be(ilp_efti_connector.Domain.Enums.GatewayProvider.MILOS);
    }

    [Fact]
    public async Task FullPipeline_ValidEvent_ShouldPersist_Customer_InDb()
    {
        var operationCode = $"CUSTPIPE-{Guid.NewGuid():N}";
        var customerCode  = $"CUST-{Guid.NewGuid():N}";
        var evt           = BuildEvent(operationCode, customerCode);

        await _harness.Bus.Publish(evt);

        await _harness.Published.Any<EftiSendRequestedEvent>(
            msg => msg.Context.Message.CorrelationId == evt.CorrelationId,
            CancellationToken.None);

        await using var scope = _provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EftiConnectorDbContext>();

        var customer = await db.Customers.FirstOrDefaultAsync(c => c.CustomerCode == customerCode);

        customer.Should().NotBeNull("il cliente deve essere creato via upsert automatico");
        customer!.AutoCreated.Should().BeTrue();
    }

    [Fact]
    public async Task FullPipeline_InvalidPayload_ShouldPublish_ValidationFailedEvent_AndNotPersist()
    {
        var evt = new TransportSubmittedEvent(
            TransportOperationId: Guid.NewGuid(),
            SourceId:             SeededSourceId,
            CorrelationId:        Guid.NewGuid().ToString(),
            RawPayloadJson:       "{ }",
            DatasetType:          "ECMR",
            SubmittedAt:          DateTime.UtcNow);

        await _harness.Bus.Publish(evt);

        (await _harness.Published.Any<TransportValidationFailedEvent>())
            .Should().BeTrue();

        (await _harness.Published.Any<EftiSendRequestedEvent>())
            .Should().BeFalse("un payload non valido non deve produrre EftiSendRequestedEvent");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static TransportSubmittedEvent BuildEvent(
        string operationCode,
        string customerCode = "CUST-PIPELINE-001")
    {
        var payload = new Dictionary<string, object?>
        {
            ["OperationCode"]   = operationCode,
            ["DatasetType"]     = "ECMR",
            ["CustomerCode"]    = customerCode,
            ["CustomerName"]    = "Pipeline Test SRL",
            ["CustomerVat"]     = "IT98765432100",
            ["CustomerEori"]    = (string?)null,
            ["DestinationCode"] = (string?)null,
            ["ConsignorAddress"]= (object?)null,
            ["Consignee"] = new Dictionary<string, object>
            {
                ["Name"]        = "Destinatario Pipeline SRL",
                ["PlayerType"]  = "CONSIGNEE",
                ["CityName"]    = "Napoli",
                ["CountryCode"] = "IT"
            },
            ["Carriers"] = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["SortOrder"]    = 0,
                    ["Name"]         = "Vettore Pipeline SRL",
                    ["PlayerType"]   = "CARRIER",
                    ["TractorPlate"] = "PI123PE",
                    ["CityName"]     = "Torino",
                    ["CountryCode"]  = "IT"
                }
            },
            ["AcceptanceLocation"] = (object?)null,
            ["DeliveryLocation"]   = (object?)null,
            ["ConsignmentItems"]   = (object?)null,
            ["TransportDetails"]   = (object?)null,
            ["Hashcode"]           = (object?)null
        };

        return new TransportSubmittedEvent(
            TransportOperationId: Guid.NewGuid(),
            SourceId:             SeededSourceId,
            CorrelationId:        Guid.NewGuid().ToString(),
            RawPayloadJson:       JsonSerializer.Serialize(payload),
            DatasetType:          "ECMR",
            SubmittedAt:          DateTime.UtcNow);
    }

    // ── Stub ICurrentUserService per i behaviour MediatR ─────────────────────
    private sealed class NullCurrentUserService : ICurrentUserService
    {
        public Guid?   UserId          => null;
        public string? Username        => "integration-test";
        public bool    IsAuthenticated => false;
    }
}

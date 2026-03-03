using ilp_efti_connector.Shared.Contracts.Events;
using ilp_efti_connector.ValidationService.Consumers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace ilp_efti_connector.IntegrationTests.Consumers;

/// <summary>
/// Test del TransportSubmittedConsumer usando MassTransit InMemory test harness.
/// Verifica che il consumer pubblichi l'evento corretto in base al payload ricevuto.
/// </summary>
public sealed class TransportSubmittedConsumerTests : IAsyncLifetime
{
    private ServiceProvider _provider = null!;
    private ITestHarness    _harness  = null!;

    public async Task InitializeAsync()
    {
        _provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<TransportSubmittedConsumer>();
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

    // ── Percorso felice ───────────────────────────────────────────────────────

    [Fact]
    public async Task Consume_ValidPayload_ShouldPublish_TransportValidatedEvent()
    {
        var evt = BuildEvent(BuildValidPayloadJson("CMR-001"));

        await _harness.Bus.Publish(evt);

        (await _harness.Published.Any<TransportValidatedEvent>())
            .Should().BeTrue("un payload valido deve produrre un TransportValidatedEvent");

        var published = _harness.Published.Select<TransportValidatedEvent>().First();
        published.Context.Message.TransportOperationId.Should().Be(evt.TransportOperationId);
        published.Context.Message.CorrelationId.Should().Be(evt.CorrelationId);
    }

    [Fact]
    public async Task Consume_ValidPayload_ShouldNotPublish_ValidationFailedEvent()
    {
        await _harness.Bus.Publish(BuildEvent(BuildValidPayloadJson("CMR-002")));

        (await _harness.Published.Any<TransportValidationFailedEvent>())
            .Should().BeFalse();
    }

    // ── Percorsi di errore ────────────────────────────────────────────────────

    [Fact]
    public async Task Consume_EmptyPayload_ShouldPublish_ValidationFailedEvent()
    {
        var evt = BuildEvent(string.Empty);

        await _harness.Bus.Publish(evt);

        (await _harness.Published.Any<TransportValidationFailedEvent>())
            .Should().BeTrue();

        var failed = _harness.Published.Select<TransportValidationFailedEvent>().First();
        failed.Context.Message.ValidationErrors.Should().Contain(e => e.Contains("RawPayloadJson"));
    }

    [Fact]
    public async Task Consume_InvalidJson_ShouldPublish_ValidationFailedEvent()
    {
        await _harness.Bus.Publish(BuildEvent("{ not valid json }"));

        (await _harness.Published.Any<TransportValidationFailedEvent>())
            .Should().BeTrue();
    }

    [Fact]
    public async Task Consume_MissingOperationCode_ShouldPublish_ValidationFailedEvent()
    {
        var payload = BuildValidPayload("CMR-MISSING");
        payload.Remove("OperationCode");
        var json = JsonSerializer.Serialize(payload);

        await _harness.Bus.Publish(BuildEvent(json));

        (await _harness.Published.Any<TransportValidationFailedEvent>())
            .Should().BeTrue();

        var failed = _harness.Published.Select<TransportValidationFailedEvent>().First();
        failed.Context.Message.ValidationErrors.Should().Contain(e => e.Contains("OperationCode"));
    }

    [Fact]
    public async Task Consume_MissingCarriers_ShouldPublish_ValidationFailedEvent()
    {
        var payload = BuildValidPayload("CMR-NO-CARRIER");
        payload["Carriers"] = new List<object>();
        var json = JsonSerializer.Serialize(payload);

        await _harness.Bus.Publish(BuildEvent(json));

        (await _harness.Published.Any<TransportValidationFailedEvent>())
            .Should().BeTrue();

        var failed = _harness.Published.Select<TransportValidationFailedEvent>().First();
        failed.Context.Message.ValidationErrors
            .Should().Contain(e => e.Contains("vettore"));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static TransportSubmittedEvent BuildEvent(string rawPayloadJson) => new(
        TransportOperationId: Guid.NewGuid(),
        SourceId:             new Guid("11111111-1111-1111-1111-111111111111"),
        CorrelationId:        Guid.NewGuid().ToString(),
        RawPayloadJson:       rawPayloadJson,
        DatasetType:          "ECMR",
        SubmittedAt:          DateTime.UtcNow);

    private static string BuildValidPayloadJson(string code)
        => JsonSerializer.Serialize(BuildValidPayload(code));

    private static Dictionary<string, object?> BuildValidPayload(string code) => new()
    {
        ["OperationCode"]  = code,
        ["DatasetType"]    = "ECMR",
        ["CustomerCode"]   = "CUST-001",
        ["CustomerName"]   = "Mittente SRL",
        ["CustomerVat"]    = "IT12345678901",
        ["CustomerEori"]   = (string?)null,
        ["DestinationCode"]= (string?)null,
        ["ConsignorAddress"]= (object?)null,
        ["Consignee"]      = new Dictionary<string, object>
        {
            ["Name"]       = "Destinatario SRL",
            ["PlayerType"] = "CONSIGNEE",
            ["CityName"]   = "Roma",
            ["CountryCode"]= "IT"
        },
        ["Carriers"] = new List<object>
        {
            new Dictionary<string, object>
            {
                ["SortOrder"]   = 0,
                ["Name"]        = "Vettore SRL",
                ["PlayerType"]  = "CARRIER",
                ["TractorPlate"]= "AB123CD",
                ["CityName"]    = "Milano",
                ["CountryCode"] = "IT"
            }
        },
        ["AcceptanceLocation"] = (object?)null,
        ["DeliveryLocation"]   = (object?)null,
        ["ConsignmentItems"]   = (object?)null,
        ["TransportDetails"]   = (object?)null,
        ["Hashcode"]           = (object?)null
    };
}

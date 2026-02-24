using ilp_efti_connector.Domain.Enums;
using ilp_efti_connector.NormalizationService.Mapping;
using ilp_efti_connector.Shared.Contracts.Dtos;
using ilp_efti_connector.Shared.Contracts.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ilp_efti_connector.NormalizationService.Consumers;

public sealed class TransportValidatedConsumer : IConsumer<TransportValidatedEvent>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters                  = { new JsonStringEnumConverter() }
    };

    private readonly IMediator _mediator;
    private readonly IPublishEndpoint _publish;
    private readonly IConfiguration _config;
    private readonly ILogger<TransportValidatedConsumer> _logger;

    public TransportValidatedConsumer(
        IMediator mediator,
        IPublishEndpoint publish,
        IConfiguration config,
        ILogger<TransportValidatedConsumer> logger)
    {
        _mediator = mediator;
        _publish  = publish;
        _config   = config;
        _logger   = logger;
    }

    public async Task Consume(ConsumeContext<TransportValidatedEvent> context)
    {
        var evt = context.Message;
        var ct  = context.CancellationToken;

        _logger.LogInformation("Normalizzazione → OperationId={Id}", evt.TransportOperationId);

        SourcePayloadDto dto;
        try
        {
            dto = JsonSerializer.Deserialize<SourcePayloadDto>(evt.RawPayloadJson, JsonOptions)
                  ?? throw new InvalidOperationException("Payload deserializzato come null.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Impossibile deserializzare il payload per OperationId={Id}", evt.TransportOperationId);
            throw;
        }

        // 1. Upsert cliente / destinazione
        var upsertResult = await _mediator.Send(
            SourcePayloadMapper.ToUpsertCustomerCommand(dto, evt.SourceId), ct);

        // 2. Costruisci EcmrPayload e serializzalo
        var ecmrPayload = SourcePayloadMapper.ToEcmrPayload(
            dto, dto.CustomerName, dto.CustomerVat, dto.CustomerEori);
        var ecmrPayloadJson = JsonSerializer.Serialize(ecmrPayload, JsonOptions);

        // 3. Determina il gateway provider
        var providerStr = _config["EftiGateway:Provider"] ?? GatewayProvider.MILOS.ToString();
        var gatewayProvider = Enum.TryParse<GatewayProvider>(providerStr, ignoreCase: true, out var gp)
            ? gp
            : GatewayProvider.MILOS;

        // 4. Crea TransportOperation + EftiMessage via MediatR
        var submitCmd = SourcePayloadMapper.ToSubmitCommand(
            dto,
            sourceId:             evt.SourceId,
            customerId:           upsertResult.CustomerId,
            destinationId:        upsertResult.DestinationId,
            transportOperationId: evt.TransportOperationId,
            gatewayProvider:      gatewayProvider,
            ecmrPayloadJson:      ecmrPayloadJson);

        var submitResult = await _mediator.Send(submitCmd, ct);

        // 5. Pubblica EftiSendRequestedEvent
        await _publish.Publish(new EftiSendRequestedEvent(
            EftiMessageId:        submitResult.EftiMessageId,
            TransportOperationId: submitResult.TransportOperationId,
            CorrelationId:        evt.CorrelationId,
            GatewayProvider:      gatewayProvider.ToString(),
            PayloadJson:          ecmrPayloadJson,
            DatasetType:          dto.DatasetType), ct);

        _logger.LogInformation(
            "Normalizzazione completata → OperationId={OpId} MessageId={MsgId} Provider={Provider}",
            submitResult.TransportOperationId, submitResult.EftiMessageId, gatewayProvider);
    }
}

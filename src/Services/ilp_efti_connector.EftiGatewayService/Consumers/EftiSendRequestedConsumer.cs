using ilp_efti_connector.Domain.Enums;
using ilp_efti_connector.Domain.Interfaces.Repositories;
using ilp_efti_connector.EftiGatewayService;
using ilp_efti_connector.Gateway.Contracts.Models;
using ilp_efti_connector.Shared.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ilp_efti_connector.EftiGatewayService.Consumers;

public sealed class EftiSendRequestedConsumer : IConsumer<EftiSendRequestedEvent>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters                  = { new JsonStringEnumConverter() }
    };

    private readonly GatewaySelector _selector;
    private readonly IEftiMessageRepository _messages;
    private readonly IUnitOfWork _uow;
    private readonly IPublishEndpoint _publish;
    private readonly ILogger<EftiSendRequestedConsumer> _logger;

    public EftiSendRequestedConsumer(
        GatewaySelector selector,
        IEftiMessageRepository messages,
        IUnitOfWork uow,
        IPublishEndpoint publish,
        ILogger<EftiSendRequestedConsumer> logger)
    {
        _selector = selector;
        _messages = messages;
        _uow      = uow;
        _publish  = publish;
        _logger   = logger;
    }

    public async Task Consume(ConsumeContext<EftiSendRequestedEvent> context)
    {
        var evt = context.Message;
        var ct  = context.CancellationToken;

        _logger.LogInformation("Invio gateway → MessageId={Id} Provider={Provider}",
            evt.EftiMessageId, evt.GatewayProvider);

        await SendToGatewayAsync(
            evt.EftiMessageId, evt.TransportOperationId, evt.CorrelationId,
            evt.GatewayProvider, evt.PayloadJson, ct);
    }

    internal async Task SendToGatewayAsync(
        Guid   eftiMessageId,
        Guid   transportOperationId,
        string correlationId,
        string gatewayProvider,
        string payloadJson,
        CancellationToken ct)
    {
        var message = await _messages.GetByIdAsync(eftiMessageId, ct);
        if (message is null)
        {
            _logger.LogWarning("EftiMessage {Id} non trovato — skip.", eftiMessageId);
            return;
        }

        EcmrPayload payload;
        try
        {
            payload = JsonSerializer.Deserialize<EcmrPayload>(payloadJson, JsonOptions)
                      ?? throw new InvalidOperationException("PayloadJson deserializzato come null.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Impossibile deserializzare PayloadJson per MessageId={Id}", eftiMessageId);
            await PublishResponseAsync(eftiMessageId, transportOperationId, correlationId,
                gatewayProvider, false, null, null, ex.Message, ct);
            return;
        }

        var gateway = _selector.Get(gatewayProvider);
        var result  = await gateway.SendEcmrAsync(payload, ct);

        // Aggiorna il messaggio
        message.SentAt    = DateTime.UtcNow;
        message.Status    = result.IsSuccess ? MessageStatus.SENT : MessageStatus.ERROR;
        message.ExternalId   = result.ExternalId;
        message.ExternalUuid = result.ExternalUuid;
        _messages.Update(message);
        await _uow.SaveChangesAsync(ct);

        await PublishResponseAsync(eftiMessageId, transportOperationId, correlationId,
            gatewayProvider, result.IsSuccess, result.ExternalId, result.ExternalUuid,
            result.ErrorMessage, ct);

        _logger.LogInformation(
            "Gateway response → MessageId={Id} Success={Ok} ExternalId={Ext}",
            eftiMessageId, result.IsSuccess, result.ExternalId);
    }

    private Task PublishResponseAsync(
        Guid eftiMessageId, Guid transportOperationId, string correlationId,
        string gatewayProvider, bool isSuccess,
        string? externalId, string? externalUuid, string? errorMessage,
        CancellationToken ct) =>
        _publish.Publish(new EftiResponseReceivedEvent(
            EftiMessageId:        eftiMessageId,
            TransportOperationId: transportOperationId,
            CorrelationId:        correlationId,
            GatewayProvider:      gatewayProvider,
            IsSuccess:            isSuccess,
            ExternalId:           externalId,
            ExternalUuid:         externalUuid,
            ErrorMessage:         errorMessage,
            ReceivedAt:           DateTime.UtcNow), ct);
}

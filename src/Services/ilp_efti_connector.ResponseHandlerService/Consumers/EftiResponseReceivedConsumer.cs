using ilp_efti_connector.Domain.Enums;
using ilp_efti_connector.Domain.Interfaces.Repositories;
using ilp_efti_connector.Shared.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ilp_efti_connector.ResponseHandlerService.Consumers;

public sealed class EftiResponseReceivedConsumer : IConsumer<EftiResponseReceivedEvent>
{
    private const int MaxRetryCount = 3;

    private readonly IEftiMessageRepository _messages;
    private readonly ITransportOperationRepository _operations;
    private readonly IUnitOfWork _uow;
    private readonly IPublishEndpoint _publish;
    private readonly ILogger<EftiResponseReceivedConsumer> _logger;

    public EftiResponseReceivedConsumer(
        IEftiMessageRepository messages,
        ITransportOperationRepository operations,
        IUnitOfWork uow,
        IPublishEndpoint publish,
        ILogger<EftiResponseReceivedConsumer> logger)
    {
        _messages   = messages;
        _operations = operations;
        _uow        = uow;
        _publish    = publish;
        _logger     = logger;
    }

    public async Task Consume(ConsumeContext<EftiResponseReceivedEvent> context)
    {
        var evt = context.Message;
        var ct  = context.CancellationToken;

        _logger.LogInformation("Gestione risposta → MessageId={Id} Success={Ok}",
            evt.EftiMessageId, evt.IsSuccess);

        var message = await _messages.GetByIdAsync(evt.EftiMessageId, ct);
        if (message is null)
        {
            _logger.LogWarning("EftiMessage {Id} non trovato — skip.", evt.EftiMessageId);
            return;
        }

        var operation = await _operations.GetByIdAsync(evt.TransportOperationId, ct);

        if (evt.IsSuccess)
        {
            message.Status        = MessageStatus.SENT;
            message.ExternalId    = evt.ExternalId;
            message.ExternalUuid  = evt.ExternalUuid;
            message.AcknowledgedAt = evt.ReceivedAt;

            if (operation is not null)
                operation.Status = TransportOperationStatus.SENT;
        }
        else
        {
            message.RetryCount++;

            if (message.RetryCount < MaxRetryCount)
            {
                // Backoff esponenziale: 2^RetryCount minuti
                message.Status      = MessageStatus.RETRY;
                message.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, message.RetryCount));

                _logger.LogWarning(
                    "EftiMessage {Id} in RETRY (tentativo {N}/{Max}), prossimo alle {Next}.",
                    evt.EftiMessageId, message.RetryCount, MaxRetryCount, message.NextRetryAt);
            }
            else
            {
                message.Status = MessageStatus.DEAD;

                if (operation is not null)
                    operation.Status = TransportOperationStatus.ERROR;

                _logger.LogError("EftiMessage {Id} → DEAD LETTER (tentativi esauriti).", evt.EftiMessageId);
            }
        }

        _messages.Update(message);
        if (operation is not null) _operations.Update(operation);
        await _uow.SaveChangesAsync(ct);

        // Notifica la sorgente solo su SENT o DEAD
        if (message.Status is MessageStatus.SENT or MessageStatus.DEAD)
        {
            await _publish.Publish(new SourceNotificationRequiredEvent(
                TransportOperationId: evt.TransportOperationId,
                SourceId:             message.SourceId,
                CorrelationId:        evt.CorrelationId,
                Status:               message.Status.ToString(),
                ExternalId:           evt.ExternalId,
                ExternalUuid:         evt.ExternalUuid,
                ErrorMessage:         evt.ErrorMessage,
                OccurredAt:           DateTime.UtcNow), ct);
        }
    }
}

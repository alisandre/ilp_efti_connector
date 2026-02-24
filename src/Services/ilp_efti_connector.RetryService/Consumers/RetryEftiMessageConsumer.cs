using ilp_efti_connector.Domain.Enums;
using ilp_efti_connector.Domain.Interfaces.Repositories;
using ilp_efti_connector.Shared.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ilp_efti_connector.RetryService.Consumers;

/// <summary>
/// Gestisce i retry manuali richiesti dall'operatore via dashboard.
/// Ripristina lo stato del messaggio e lo reinvia alla pipeline gateway.
/// </summary>
public sealed class RetryEftiMessageConsumer : IConsumer<RetryEftiMessageCommand>
{
    private readonly IEftiMessageRepository _messages;
    private readonly IUnitOfWork _uow;
    private readonly IPublishEndpoint _publish;
    private readonly ILogger<RetryEftiMessageConsumer> _logger;

    public RetryEftiMessageConsumer(
        IEftiMessageRepository messages,
        IUnitOfWork uow,
        IPublishEndpoint publish,
        ILogger<RetryEftiMessageConsumer> logger)
    {
        _messages = messages;
        _uow      = uow;
        _publish  = publish;
        _logger   = logger;
    }

    public async Task Consume(ConsumeContext<RetryEftiMessageCommand> context)
    {
        var cmd = context.Message;
        var ct  = context.CancellationToken;

        _logger.LogInformation(
            "Retry manuale richiesto → MessageId={Id} RequestedBy={By}",
            cmd.EftiMessageId, cmd.RequestedBy);

        var message = await _messages.GetByIdAsync(cmd.EftiMessageId, ct);
        if (message is null)
        {
            _logger.LogWarning("EftiMessage {Id} non trovato — retry ignorato.", cmd.EftiMessageId);
            return;
        }

        if (message.Status is not (MessageStatus.DEAD or MessageStatus.ERROR))
        {
            _logger.LogWarning(
                "EftiMessage {Id} in stato {Status} — retry non applicabile.",
                cmd.EftiMessageId, message.Status);
            return;
        }

        // Ripristina il contatore e rimette in coda
        message.Status      = MessageStatus.RETRY;
        message.RetryCount  = 0;
        message.NextRetryAt = DateTime.UtcNow;
        _messages.Update(message);
        await _uow.SaveChangesAsync(ct);

        await _publish.Publish(new SendToGatewayCommand(
            EftiMessageId:        message.Id,
            TransportOperationId: message.TransportOperationId,
            CorrelationId:        message.CorrelationId.ToString(),
            GatewayProvider:      message.GatewayProvider.ToString(),
            PayloadJson:          message.PayloadJson,
            DatasetType:          message.DatasetType), ct);

        _logger.LogInformation(
            "Retry manuale avviato → MessageId={Id} Provider={Provider}",
            message.Id, message.GatewayProvider);
    }
}

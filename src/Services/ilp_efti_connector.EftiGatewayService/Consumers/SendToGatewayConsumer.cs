using ilp_efti_connector.Domain.Interfaces.Repositories;
using ilp_efti_connector.EftiGatewayService.Consumers;
using ilp_efti_connector.Shared.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ilp_efti_connector.EftiGatewayService.Consumers;

/// <summary>
/// Gestisce i comandi di retry manuale e automatico (<see cref="SendToGatewayCommand"/>)
/// inviati dal RetryService o dall'operatore.
/// </summary>
public sealed class SendToGatewayConsumer : IConsumer<SendToGatewayCommand>
{
    private readonly EftiSendRequestedConsumer _inner;
    private readonly ILogger<SendToGatewayConsumer> _logger;

    public SendToGatewayConsumer(
        EftiSendRequestedConsumer inner,
        ILogger<SendToGatewayConsumer> logger)
    {
        _inner  = inner;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SendToGatewayCommand> context)
    {
        var cmd = context.Message;
        _logger.LogInformation("Retry gateway → MessageId={Id}", cmd.EftiMessageId);

        await _inner.SendToGatewayAsync(
            cmd.EftiMessageId, cmd.TransportOperationId, cmd.CorrelationId,
            cmd.GatewayProvider, cmd.PayloadJson, context.CancellationToken);
    }
}

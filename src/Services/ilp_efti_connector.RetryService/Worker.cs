using ilp_efti_connector.Domain.Interfaces.Repositories;
using ilp_efti_connector.Shared.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace ilp_efti_connector.RetryService;

/// <summary>
/// Polling periodico dei messaggi in stato RETRY con <c>NextRetryAt &lt;= utcNow</c>.
/// Per ogni messaggio trovato invia un <see cref="SendToGatewayCommand"/> all'EftiGatewayService.
/// </summary>
public sealed class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Worker> _logger;
    private readonly TimeSpan _pollInterval;

    public Worker(IServiceScopeFactory scopeFactory, ILogger<Worker> logger, IConfiguration config)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
        _pollInterval = TimeSpan.FromSeconds(config.GetValue("RetryService:PollIntervalSeconds", 30));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RetryService avviato (intervallo: {Interval}s).", _pollInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingRetriesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Errore nel ciclo di retry.");
            }

            await Task.Delay(_pollInterval, stoppingToken);
        }
    }

    private async Task ProcessPendingRetriesAsync(CancellationToken ct)
    {
        await using var scope    = _scopeFactory.CreateAsyncScope();
        var messageRepo          = scope.ServiceProvider.GetRequiredService<IEftiMessageRepository>();
        var sendEndpointProvider = scope.ServiceProvider.GetRequiredService<ISendEndpointProvider>();
        
        _logger.LogDebug("RetryService: avvio ciclo di polling.");

        // Messaggi in RETRY con NextRetryAt scaduto
        var retryMessages = await messageRepo.GetPendingForRetryAsync(DateTime.UtcNow, ct);

        // Messaggi in PENDING bloccati da >5 min (EftiSendRequestedEvent perso al publish)
        var stuckMessages = await messageRepo.GetStuckPendingAsync(TimeSpan.FromMinutes(5), ct);

        var allMessages = retryMessages.Concat(stuckMessages).ToList();
        if (allMessages.Count == 0) return;

        _logger.LogInformation(
            "RetryService: {Retry} RETRY + {Stuck} PENDING bloccati da reinviare.",
            retryMessages.Count, stuckMessages.Count);

        foreach (var msg in allMessages)
        {
            var cmd = new SendToGatewayCommand(
                EftiMessageId:        msg.Id,
                TransportOperationId: msg.TransportOperationId,
                CorrelationId:        msg.CorrelationId.ToString(),
                GatewayProvider:      msg.GatewayProvider.ToString(),
                PayloadJson:          msg.PayloadJson,
                DatasetType:          msg.DatasetType);

            var endpoint = await sendEndpointProvider.GetSendEndpoint(
                new Uri("queue:efti-send-requested"));

            await endpoint.Send(cmd, ct);

            _logger.LogDebug("RetryService: SendToGatewayCommand inviato per MessageId={Id}.", msg.Id);
        }
    }
}


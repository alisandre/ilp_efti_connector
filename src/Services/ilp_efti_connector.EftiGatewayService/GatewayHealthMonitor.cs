using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ilp_efti_connector.EftiGatewayService;

/// <summary>
/// Servizio in background che verifica periodicamente la raggiungibilità
/// di tutti i gateway registrati (MILOS e EFTI_NATIVE) e ne registra lo stato.
/// </summary>
public sealed class GatewayHealthMonitor : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);
    private static readonly string[] Providers     = ["MILOS", "EFTI_NATIVE"];

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GatewayHealthMonitor> _logger;

    public GatewayHealthMonitor(
        IServiceScopeFactory scopeFactory,
        ILogger<GatewayHealthMonitor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "GatewayHealthMonitor avviato — intervallo controllo: {Interval}", CheckInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckAllGatewaysAsync(stoppingToken);

            try
            {
                await Task.Delay(CheckInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task CheckAllGatewaysAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var selector = scope.ServiceProvider.GetRequiredService<GatewaySelector>();

        foreach (var provider in Providers)
        {
            try
            {
                var gateway = selector.Get(provider);
                var status  = await gateway.HealthCheckAsync(ct);

                if (status.IsHealthy)
                    _logger.LogInformation(
                        "Gateway {Provider} HEALTHY — responseTime={Ms:F0}ms",
                        provider, status.ResponseTime.TotalMilliseconds);
                else
                    _logger.LogWarning(
                        "Gateway {Provider} UNHEALTHY — error={Error} responseTime={Ms:F0}ms",
                        provider, status.ErrorMessage, status.ResponseTime.TotalMilliseconds);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Errore imprevisto durante il controllo salute di {Provider}", provider);
            }
        }
    }
}

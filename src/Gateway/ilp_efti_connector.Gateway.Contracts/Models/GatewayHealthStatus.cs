namespace ilp_efti_connector.Gateway.Contracts.Models;

/// <summary>
/// Stato di salute del gateway EFTI attivo, restituito da <see cref="IEftiGateway.HealthCheckAsync"/>.
/// </summary>
public record GatewayHealthStatus(
    bool     IsHealthy,
    string   Provider,
    string?  ErrorMessage,
    TimeSpan ResponseTime,
    DateTime CheckedAt
)
{
    public static GatewayHealthStatus Healthy(string provider, TimeSpan responseTime)
        => new(true, provider, null, responseTime, DateTime.UtcNow);

    public static GatewayHealthStatus Unhealthy(string provider, string errorMessage, TimeSpan responseTime)
        => new(false, provider, errorMessage, responseTime, DateTime.UtcNow);
}

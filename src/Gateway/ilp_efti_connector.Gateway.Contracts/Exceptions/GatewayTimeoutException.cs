namespace ilp_efti_connector.Gateway.Contracts.Exceptions;

/// <summary>
/// Eccezione sollevata quando la chiamata al gateway supera il timeout configurato.
/// Il messaggio viene messo automaticamente in coda per retry dal RetryService.
/// </summary>
public sealed class GatewayTimeoutException : GatewayException
{
    public TimeSpan ConfiguredTimeout { get; }

    public GatewayTimeoutException(string provider, TimeSpan configuredTimeout)
        : base(provider, $"Il gateway '{provider}' non ha risposto entro {configuredTimeout.TotalSeconds:0}s.")
    {
        ConfiguredTimeout = configuredTimeout;
    }

    public GatewayTimeoutException(string provider, TimeSpan configuredTimeout, Exception inner)
        : base(provider, $"Il gateway '{provider}' non ha risposto entro {configuredTimeout.TotalSeconds:0}s.", inner)
    {
        ConfiguredTimeout = configuredTimeout;
    }
}

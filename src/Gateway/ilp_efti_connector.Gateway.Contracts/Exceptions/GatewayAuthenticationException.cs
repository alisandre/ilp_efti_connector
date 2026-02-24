namespace ilp_efti_connector.Gateway.Contracts.Exceptions;

/// <summary>
/// Eccezione sollevata quando l'autenticazione verso il gateway fallisce.
/// Fase 1: API Key non valida o scaduta (MILOS).
/// Fase 2: Token OAuth2 non ottenuto o certificato X.509 rifiutato (EFTI Native).
/// </summary>
public sealed class GatewayAuthenticationException : GatewayException
{
    public GatewayAuthenticationException(string provider, string message)
        : base(provider, message, httpStatusCode: 401) { }

    public GatewayAuthenticationException(string provider, string message, Exception inner)
        : base(provider, message, inner, httpStatusCode: 401) { }
}

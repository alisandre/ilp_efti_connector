namespace ilp_efti_connector.Gateway.EftiNative;

/// <summary>
/// Configurazione per il gateway EFTI Nativo (Fase 2).
/// Letta da appsettings.json alla sezione <c>EftiGateway:EftiNative</c>.
/// </summary>
public sealed class EftiNativeOptions
{
    /// <summary>Base URL dell'EFTI Gate nazionale. Es: https://efti-gate.example.eu/</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Endpoint OAuth2 token. Es: https://auth.efti.eu/oauth2/token</summary>
    public string TokenEndpoint { get; set; } = string.Empty;

    /// <summary>Client ID OAuth2.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>Client secret (flow client_credentials semplice). Null se si usa X.509.</summary>
    public string? ClientSecret { get; set; }

    /// <summary>Percorso del certificato X.509 (.pfx / .p12) per mutual TLS e client_assertion.</summary>
    public string? CertificatePath { get; set; }

    /// <summary>Password del certificato X.509.</summary>
    public string? CertificatePassword { get; set; }

    /// <summary>Scope OAuth2 (default: efti).</summary>
    public string Scope { get; set; } = "efti";

    /// <summary>Timeout in secondi per le chiamate HTTP verso EFTI Gate (default: 30).</summary>
    public int TimeoutSeconds { get; set; } = 30;
}

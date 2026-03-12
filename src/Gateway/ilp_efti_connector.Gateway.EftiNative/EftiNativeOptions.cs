namespace ilp_efti_connector.Gateway.EftiNative;

/// <summary>
/// Configurazione per il gateway EFTI Nativo (Fase 2).
/// Letta da appsettings.json alla sezione <c>EftiGateway:EftiNative</c>.
/// </summary>
public sealed class EftiNativeOptions
{
    /// <summary>Base URL dell'EFTI Gate nazionale. Es: https://efti-gate.mit.gov.it/api/</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Endpoint OAuth2 token. Es: https://auth.efti.mit.gov.it/oauth2/token</summary>
    public string TokenEndpoint { get; set; } = string.Empty;

    /// <summary>Client ID OAuth2 registrato presso l'Identity Provider del Gate.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Client secret OAuth2 (solo per ambienti di sviluppo/test).
    /// Null se si usa <see cref="UseClientAssertion"/> con X.509 (obbligatorio in produzione eIDAS).
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Se true, usa <c>client_assertion</c> JWT firmato con chiave privata X.509
    /// invece del <c>client_secret</c> (RFC 7523 — obbligatorio per produzione eIDAS).
    /// </summary>
    public bool UseClientAssertion { get; set; } = false;

    /// <summary>Percorso del certificato X.509 (.pfx / .p12) per mutual TLS e client_assertion.</summary>
    public string? CertificatePath { get; set; }

    /// <summary>Password del certificato X.509.</summary>
    public string? CertificatePassword { get; set; }

    /// <summary>Scope OAuth2 (default: efti).</summary>
    public string Scope { get; set; } = "efti";

    /// <summary>
    /// Codice paese ISO 3166-1 alpha-2 per la generazione dell'UID eFTI.
    /// Es: IT per l'Italia. Fa parte del formato: <c>{CC}.{PlatformId}.{DatasetType}.{UniqueId}</c>.
    /// </summary>
    public string CountryCode { get; set; } = "IT";

    /// <summary>
    /// Identificatore della piattaforma TFP per la generazione dell'UID eFTI.
    /// Registrato presso il Gate nazionale. Es: EFTI_CONNECTOR.
    /// </summary>
    public string PlatformId { get; set; } = "EFTI_CONNECTOR";

    /// <summary>
    /// URL pubblico del Connector per ricevere i callback di stato dal Gate EFTI (webhook asincrono).
    /// Es: https://connector.azienda.it/api/efti/callback
    /// </summary>
    public string? WebhookCallbackUrl { get; set; }

    /// <summary>Timeout in secondi per le chiamate HTTP verso EFTI Gate (default: 30).</summary>
    public int TimeoutSeconds { get; set; } = 30;
}

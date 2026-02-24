namespace ilp_efti_connector.Gateway.Milos;

/// <summary>
/// Configurazione per il gateway MILOS TFP (Circle SpA).
/// Letta da appsettings.json alla sezione <c>EftiGateway:Milos</c>.
/// </summary>
public sealed class MilosGatewayOptions
{
    /// <summary>Base URL dell'e-CMR Service MILOS. Es: https://&lt;server&gt;/api/ecmr-service/</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>API Key per autenticazione verso MILOS (header X-API-Key).</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Timeout in secondi per le chiamate HTTP (default: 30).</summary>
    public int TimeoutSeconds { get; set; } = 30;
}

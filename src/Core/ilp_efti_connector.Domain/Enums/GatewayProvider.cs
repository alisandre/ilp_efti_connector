namespace ilp_efti_connector.Domain.Enums;

/// <summary>
/// Provider del gateway EFTI utilizzato.
/// </summary>
public enum GatewayProvider
{
    /// <summary>
    /// Integrazione tramite MILOS TFP (Fase 1).
    /// </summary>
    MILOS,

    /// <summary>
    /// Integrazione diretta con EFTI Gate nazionale (Fase 2).
    /// </summary>
    EFTI_NATIVE
}

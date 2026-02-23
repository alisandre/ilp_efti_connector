namespace ilp_efti_connector.Domain.Enums;

/// <summary>
/// Tipologia di sistema sorgente.
/// </summary>
public enum SourceType
{
    /// <summary>
    /// Transport Management System.
    /// </summary>
    TMS,

    /// <summary>
    /// Warehouse Management System.
    /// </summary>
    WMS,

    /// <summary>
    /// Enterprise Resource Planning.
    /// </summary>
    ERP,

    /// <summary>
    /// Sistema doganale.
    /// </summary>
    CUSTOMS,

    /// <summary>
    /// Inserimento manuale da interfaccia utente.
    /// </summary>
    MANUAL
}

namespace ilp_efti_connector.Domain.Enums;

/// <summary>
/// Stato dell'operazione di trasporto.
/// </summary>
public enum TransportOperationStatus
{
    /// <summary>
    /// Bozza, non ancora inviata.
    /// </summary>
    DRAFT,

    /// <summary>
    /// In attesa di validazione.
    /// </summary>
    PENDING_VALIDATION,

    /// <summary>
    /// Validata, pronta per l'invio.
    /// </summary>
    VALIDATED,

    /// <summary>
    /// In invio al gateway EFTI.
    /// </summary>
    SENDING,

    /// <summary>
    /// Inviata con successo.
    /// </summary>
    SENT,

    /// <summary>
    /// Riconosciuta dal sistema EFTI.
    /// </summary>
    ACKNOWLEDGED,

    /// <summary>
    /// Errore nell'elaborazione.
    /// </summary>
    ERROR,

    /// <summary>
    /// Operazione annullata.
    /// </summary>
    CANCELLED
}

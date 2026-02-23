namespace ilp_efti_connector.Domain.Enums;

/// <summary>
/// Stato del messaggio EFTI nel suo ciclo di vita.
/// </summary>
public enum MessageStatus
{
    /// <summary>
    /// Messaggio creato, in attesa di invio.
    /// </summary>
    PENDING,

    /// <summary>
    /// Messaggio inviato con successo al gateway.
    /// </summary>
    SENT,

    /// <summary>
    /// Messaggio riconosciuto (ACK ricevuto).
    /// </summary>
    ACKNOWLEDGED,

    /// <summary>
    /// Errore nell'invio o nella risposta.
    /// </summary>
    ERROR,

    /// <summary>
    /// In coda per retry automatico.
    /// </summary>
    RETRY,

    /// <summary>
    /// Messaggio definitivamente fallito (spostato in dead letter queue).
    /// </summary>
    DEAD
}

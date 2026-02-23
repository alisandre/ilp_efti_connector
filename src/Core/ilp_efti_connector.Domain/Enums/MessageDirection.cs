namespace ilp_efti_connector.Domain.Enums;

/// <summary>
/// Direzione del messaggio EFTI.
/// </summary>
public enum MessageDirection
{
    /// <summary>
    /// Messaggio in ingresso (ricevuto da EFTI o MILOS).
    /// </summary>
    INBOUND,

    /// <summary>
    /// Messaggio in uscita (inviato a EFTI o MILOS).
    /// </summary>
    OUTBOUND
}

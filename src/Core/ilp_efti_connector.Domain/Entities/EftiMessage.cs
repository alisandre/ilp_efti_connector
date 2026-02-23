namespace ilp_efti_connector.Domain.Entities;

/// <summary>
/// Rappresenta un messaggio EFTI scambiato con il gateway esterno.
/// Mappa la tabella 'efti_messages' del database.
/// </summary>
public class EftiMessage
{
    /// <summary>
    /// ID del messaggio interno.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID della sorgente che ha originato il messaggio.
    /// </summary>
    public Guid SourceId { get; set; }

    /// <summary>
    /// ID dell'operazione di trasporto collegata.
    /// </summary>
    public Guid TransportOperationId { get; set; }

    /// <summary>
    /// ID di correlazione end-to-end per tracciamento distribuito.
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    /// Provider gateway utilizzato per questo messaggio.
    /// </summary>
    public Enums.GatewayProvider GatewayProvider { get; set; }

    /// <summary>
    /// Direzione del messaggio (inbound/outbound).
    /// </summary>
    public Enums.MessageDirection Direction { get; set; }

    /// <summary>
    /// Tipo di dataset EFTI (eCMR, eDDT, eAWB, eRSD, eBL, eDAD).
    /// </summary>
    public string DatasetType { get; set; } = string.Empty;

    /// <summary>
    /// Stato corrente del messaggio.
    /// </summary>
    public Enums.MessageStatus Status { get; set; }

    /// <summary>
    /// Payload inviato al gateway in formato JSON.
    /// </summary>
    public string PayloadJson { get; set; } = string.Empty;

    /// <summary>
    /// ID esterno del messaggio.
    /// Fase 1 (MILOS): eCMRID
    /// Fase 2 (EFTI): messageId
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// UUID esterno dalla risposta.
    /// Fase 1 (MILOS): uuid dalla ECMRResponse
    /// Fase 2: può contenere riferimenti aggiuntivi
    /// </summary>
    public string? ExternalUuid { get; set; }

    /// <summary>
    /// Numero di tentativi di invio effettuati.
    /// </summary>
    public short RetryCount { get; set; } = 0;

    /// <summary>
    /// Timestamp del prossimo tentativo di retry (backoff esponenziale).
    /// </summary>
    public DateTime? NextRetryAt { get; set; }

    /// <summary>
    /// Timestamp dell'invio del messaggio al gateway.
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// Timestamp del riconoscimento (ACK) ricevuto.
    /// </summary>
    public DateTime? AcknowledgedAt { get; set; }

    /// <summary>
    /// Timestamp di creazione del record.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Source Source { get; set; } = null!;
    public TransportOperation TransportOperation { get; set; } = null!;
}

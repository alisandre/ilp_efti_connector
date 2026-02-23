namespace ilp_efti_connector.Domain.Entities;

/// <summary>
/// Rappresenta un'operazione di trasporto merci.
/// Mappa la tabella 'transport_operations' del database.
/// </summary>
public class TransportOperation
{
    /// <summary>
    /// ID univoco dell'operazione di trasporto.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID della sorgente che ha creato l'operazione.
    /// </summary>
    public Guid SourceId { get; set; }

    /// <summary>
    /// ID del cliente/mittente.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// ID della destinazione di consegna.
    /// </summary>
    public Guid? DestinationId { get; set; }

    /// <summary>
    /// Codice operazione (es. numero CMR, DDT).
    /// Corrisponde a eCMRID in MILOS.
    /// </summary>
    public string OperationCode { get; set; } = string.Empty;

    /// <summary>
    /// Tipo di dataset (ECMR, EDDT, ecc.).
    /// </summary>
    public string DatasetType { get; set; } = string.Empty;

    /// <summary>
    /// Stato corrente dell'operazione.
    /// </summary>
    public Enums.TransportOperationStatus Status { get; set; }

    /// <summary>
    /// Hashcode del payload (SHA-256) per verifica integrità.
    /// </summary>
    public string? Hashcode { get; set; }

    /// <summary>
    /// Algoritmo utilizzato per l'hashcode (es. SHA-256).
    /// </summary>
    public string? HashcodeAlgorithm { get; set; }

    /// <summary>
    /// Payload JSON grezzo dell'operazione al momento dell'ultimo invio al gateway.
    /// Conservato per debug e audit tecnico; non sostituisce i dati strutturati.
    /// </summary>
    public string? RawPayloadJson { get; set; }

    /// <summary>
    /// Data/ora di creazione dell'operazione.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Data/ora dell'ultimo aggiornamento.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// ID dell'utente che ha creato l'operazione (se da form manuale).
    /// </summary>
    public Guid? CreatedByUserId { get; set; }

    /// <summary>
    /// ID dell'utente che ha effettuato l'ultimo aggiornamento.
    /// </summary>
    public Guid? UpdatedByUserId { get; set; }

    // Navigation properties
    public Source Source { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public CustomerDestination? Destination { get; set; }
    public User? CreatedByUser { get; set; }
    public User? UpdatedByUser { get; set; }

    // Dati strutturati del payload (sostituiscono le colonne JSON)
    public TransportConsignee? Consignee { get; set; }
    public TransportDetail? Detail { get; set; }
    public TransportConsignmentItem? ConsignmentItem { get; set; }
    public ICollection<TransportCarrier> Carriers { get; set; } = new List<TransportCarrier>();
    public ICollection<EftiMessage> EftiMessages { get; set; } = new List<EftiMessage>();
}

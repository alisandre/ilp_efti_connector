namespace ilp_efti_connector.Domain.Entities;

/// <summary>
/// Rappresenta un cliente/mittente nell'anagrafica interna.
/// Mappa la tabella 'customers' del database.
/// </summary>
public class Customer
{
    /// <summary>
    /// UUID interno del cliente.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Codice cliente dal sistema sorgente - chiave di lookup.
    /// Deve essere univoco.
    /// </summary>
    public string CustomerCode { get; set; } = string.Empty;

    /// <summary>
    /// Ragione sociale del mittente.
    /// </summary>
    public string BusinessName { get; set; } = string.Empty;

    /// <summary>
    /// Partita IVA / VAT number.
    /// </summary>
    public string? VatNumber { get; set; }

    /// <summary>
    /// Codice EORI (obbligatorio per EFTI cross-border).
    /// </summary>
    public string? EoriCode { get; set; }

    /// <summary>
    /// Email operativa del cliente.
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Indica se il cliente è attivo.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// True se il cliente è stato creato automaticamente da un ordine.
    /// </summary>
    public bool AutoCreated { get; set; } = false;

    /// <summary>
    /// ID della sorgente che ha generato la creazione automatica.
    /// </summary>
    public Guid? SourceId { get; set; }

    /// <summary>
    /// Data di prima creazione del record.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Data dell'ultimo aggiornamento.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Source? Source { get; set; }
    public ICollection<CustomerDestination> Destinations { get; set; } = new List<CustomerDestination>();
    public ICollection<TransportOperation> TransportOperations { get; set; } = new List<TransportOperation>();
}

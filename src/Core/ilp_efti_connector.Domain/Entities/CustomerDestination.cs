namespace ilp_efti_connector.Domain.Entities;

/// <summary>
/// Rappresenta una destinazione di consegna associata a un cliente.
/// Mappa la tabella 'customer_destinations' del database.
/// </summary>
public class CustomerDestination
{
    /// <summary>
    /// UUID interno della destinazione.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID del cliente proprietario della destinazione.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Codice univoco della destinazione dal sistema sorgente.
    /// </summary>
    public string DestinationCode { get; set; } = string.Empty;

    /// <summary>
    /// Etichetta leggibile della destinazione.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Via e numero civico.
    /// </summary>
    public string AddressLine1 { get; set; } = string.Empty;

    /// <summary>
    /// Città.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Codice postale (CAP).
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Provincia / Regione.
    /// </summary>
    public string? Province { get; set; }

    /// <summary>
    /// Codice paese ISO 3166-1 alpha-2 (es. IT, ES, DE).
    /// </summary>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>
    /// Codice UN/LOCODE per identificazione geografica standardizzata.
    /// </summary>
    public string? UnLocode { get; set; }

    /// <summary>
    /// Indica se questa è la destinazione predefinita per il cliente.
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// True se la destinazione è stata creata automaticamente.
    /// </summary>
    public bool AutoCreated { get; set; } = false;

    /// <summary>
    /// Data di creazione della destinazione.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Data dell'ultimo aggiornamento.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Customer Customer { get; set; } = null!;
    public ICollection<TransportOperation> TransportOperations { get; set; } = new List<TransportOperation>();
}

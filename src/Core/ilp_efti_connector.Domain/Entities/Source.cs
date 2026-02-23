namespace ilp_efti_connector.Domain.Entities;

/// <summary>
/// Rappresenta un sistema sorgente che invia dati al Connector.
/// Mappa la tabella 'sources' del database.
/// </summary>
public class Source
{
    /// <summary>
    /// Identificatore univoco della sorgente.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Codice breve della sorgente (es. TMS_ACME).
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Nome descrittivo della sorgente.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tipologia di sistema sorgente.
    /// </summary>
    public Enums.SourceType Type { get; set; }

    /// <summary>
    /// Hash SHA-256 della API key per autenticazione.
    /// </summary>
    public string? ApiKeyHash { get; set; }

    /// <summary>
    /// Indica se la sorgente è abilitata.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Configurazione specifica in formato JSON (mapping, webhook, ecc.).
    /// </summary>
    public string? ConfigJson { get; set; }

    /// <summary>
    /// Data di creazione della sorgente.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<TransportOperation> TransportOperations { get; set; } = new List<TransportOperation>();
    public ICollection<EftiMessage> EftiMessages { get; set; } = new List<EftiMessage>();
}

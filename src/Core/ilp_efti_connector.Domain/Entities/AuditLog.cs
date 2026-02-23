namespace ilp_efti_connector.Domain.Entities;

/// <summary>
/// Rappresenta un log di audit immutabile per conformità GDPR.
/// Mappa la tabella 'audit_logs' del database.
/// </summary>
public class AuditLog
{
    /// <summary>
    /// ID univoco del log di audit.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tipo di entità interessata dall'azione.
    /// </summary>
    public Enums.AuditEntityType EntityType { get; set; }

    /// <summary>
    /// ID dell'entità specifica.
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// Tipo di azione eseguita.
    /// </summary>
    public Enums.AuditActionType ActionType { get; set; }

    /// <summary>
    /// ID dell'utente che ha eseguito l'azione.
    /// </summary>
    public Guid? PerformedByUserId { get; set; }

    /// <summary>
    /// ID della sorgente che ha eseguito l'azione (se applicabile).
    /// </summary>
    public Guid? PerformedBySourceId { get; set; }

    /// <summary>
    /// Descrizione testuale dell'azione.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Dati prima della modifica (JSON).
    /// </summary>
    public string? OldValueJson { get; set; }

    /// <summary>
    /// Dati dopo la modifica (JSON).
    /// </summary>
    public string? NewValueJson { get; set; }

    /// <summary>
    /// Indirizzo IP da cui è stata eseguita l'azione.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User Agent del client.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Timestamp dell'azione (immutabile).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User? PerformedByUser { get; set; }
}

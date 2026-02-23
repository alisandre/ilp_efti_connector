using ilp_efti_connector.Domain.Entities;

namespace ilp_efti_connector.Domain.Entities;

/// <summary>
/// Rappresenta un utente del sistema.
/// Mappa la tabella 'users' del database.
/// </summary>
public class User
{
    /// <summary>
    /// ID univoco dell'utente.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Username univoco.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email dell'utente.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Nome completo dell'utente.
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Indica se l'utente è attivo.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// ID esterno da Keycloak (subject).
    /// </summary>
    public string? KeycloakId { get; set; }

    /// <summary>
    /// Ruoli dell'utente in formato JSON (RBAC).
    /// </summary>
    public string? RolesJson { get; set; }

    /// <summary>
    /// Data di creazione dell'utente.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Data dell'ultimo accesso.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public ICollection<TransportOperation> CreatedTransportOperations { get; set; } = new List<TransportOperation>();
    public ICollection<TransportOperation> UpdatedTransportOperations { get; set; } = new List<TransportOperation>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}

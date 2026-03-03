namespace ilp_efti_connector.Application.Common.Interfaces;

/// <summary>
/// Marker per i risultati dei comandi auditabili il cui <c>EntityId</c>
/// viene generato durante l'esecuzione (es. operazioni di creazione).
/// <c>AuditBehaviour</c> usa questo valore per popolare <c>AuditLog.EntityId</c>.
/// </summary>
public interface IAuditableResult
{
    Guid AuditEntityId { get; }
}

using ilp_efti_connector.Domain.Enums;

namespace ilp_efti_connector.Application.Common.Interfaces;

/// <summary>
/// Marker per i comandi che devono generare automaticamente una riga di Audit Log
/// tramite <c>AuditBehaviour</c>. Fornisce il contesto dell'operazione.
/// </summary>
public interface IAuditableCommand
{
    AuditEntityType EntityType   { get; }
    AuditActionType ActionType   { get; }
    string          AuditDescription { get; }
}

/// <summary>
/// Specializzazione di <see cref="IAuditableCommand"/> per i comandi che conoscono
/// l'EntityId prima dell'esecuzione (es. <c>UpdateCustomerCommand</c>).
/// </summary>
public interface IAuditableCommandWithEntityId : IAuditableCommand
{
    Guid EntityId { get; }
}

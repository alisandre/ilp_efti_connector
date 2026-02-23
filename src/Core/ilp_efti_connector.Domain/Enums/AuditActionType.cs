namespace ilp_efti_connector.Domain.Enums;

/// <summary>
/// Tipo di azione eseguita, tracciata nell'audit log.
/// </summary>
public enum AuditActionType
{
    Create,
    Read,
    Update,
    Delete,
    Send,
    Receive,
    Query,
    Export
}

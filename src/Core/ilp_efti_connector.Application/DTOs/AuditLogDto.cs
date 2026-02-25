using ilp_efti_connector.Domain.Enums;

namespace ilp_efti_connector.Application.DTOs;

public sealed record AuditLogDto(
    Guid            Id,
    AuditEntityType EntityType,
    Guid            EntityId,
    AuditActionType ActionType,
    Guid?           PerformedByUserId,
    Guid?           PerformedBySourceId,
    string          Description,
    string?         OldValueJson,
    string?         NewValueJson,
    string?         IpAddress,
    string?         UserAgent,
    DateTime        CreatedAt
);

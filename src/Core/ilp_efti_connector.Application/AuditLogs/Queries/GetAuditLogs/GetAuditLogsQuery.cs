using ilp_efti_connector.Application.DTOs;
using ilp_efti_connector.Domain.Enums;
using MediatR;

namespace ilp_efti_connector.Application.AuditLogs.Queries.GetAuditLogs;

public sealed record GetAuditLogsQuery(
    AuditEntityType? EntityType,
    Guid?            EntityId,
    AuditActionType? ActionType,
    Guid?            PerformedByUserId,
    DateTime?        From,
    DateTime?        To,
    int              Page     = 1,
    int              PageSize = 20
) : IRequest<(IReadOnlyList<AuditLogDto> Items, int TotalCount)>;

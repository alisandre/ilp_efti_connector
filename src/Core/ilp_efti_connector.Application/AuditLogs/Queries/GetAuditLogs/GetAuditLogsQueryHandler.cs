using ilp_efti_connector.Application.DTOs;
using ilp_efti_connector.Domain.Interfaces.Repositories;
using MediatR;

namespace ilp_efti_connector.Application.AuditLogs.Queries.GetAuditLogs;

public sealed class GetAuditLogsQueryHandler
    : IRequestHandler<GetAuditLogsQuery, (IReadOnlyList<AuditLogDto> Items, int TotalCount)>
{
    private readonly IAuditLogRepository _auditLogs;

    public GetAuditLogsQueryHandler(IAuditLogRepository auditLogs)
        => _auditLogs = auditLogs;

    public async Task<(IReadOnlyList<AuditLogDto> Items, int TotalCount)> Handle(
        GetAuditLogsQuery query, CancellationToken ct)
    {
        var page     = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var (items, total) = await _auditLogs.GetPagedAsync(
            query.EntityType,
            query.EntityId,
            query.ActionType,
            query.PerformedByUserId,
            query.From,
            query.To,
            page,
            pageSize,
            ct);

        var dtos = items.Select(a => new AuditLogDto(
            a.Id,
            a.EntityType,
            a.EntityId,
            a.ActionType,
            a.PerformedByUserId,
            a.PerformedBySourceId,
            a.Description,
            a.OldValueJson,
            a.NewValueJson,
            a.IpAddress,
            a.UserAgent,
            a.CreatedAt)).ToList();

        return (dtos, total);
    }
}

using ilp_efti_connector.Domain.Entities;
using ilp_efti_connector.Domain.Enums;

namespace ilp_efti_connector.Domain.Interfaces.Repositories;

public interface IAuditLogRepository
{
    Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> GetPagedAsync(
        AuditEntityType? entityType,
        Guid?            entityId,
        AuditActionType? actionType,
        Guid?            performedByUserId,
        DateTime?        from,
        DateTime?        to,
        int              page,
        int              pageSize,
        CancellationToken ct = default);
    Task AddAsync(AuditLog log, CancellationToken ct = default);
}

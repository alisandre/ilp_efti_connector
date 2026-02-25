using ilp_efti_connector.Domain.Entities;
using ilp_efti_connector.Domain.Enums;
using ilp_efti_connector.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ilp_efti_connector.Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly EftiConnectorDbContext _db;

    public AuditLogRepository(EftiConnectorDbContext db) => _db = db;

    public async Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.AuditLogs.FindAsync([id], ct);

    public async Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> GetPagedAsync(
        AuditEntityType? entityType,
        Guid?            entityId,
        AuditActionType? actionType,
        Guid?            performedByUserId,
        DateTime?        from,
        DateTime?        to,
        int              page,
        int              pageSize,
        CancellationToken ct = default)
    {
        var query = _db.AuditLogs.AsNoTracking().AsQueryable();

        if (entityType.HasValue)        query = query.Where(a => a.EntityType        == entityType.Value);
        if (entityId.HasValue)          query = query.Where(a => a.EntityId           == entityId.Value);
        if (actionType.HasValue)        query = query.Where(a => a.ActionType         == actionType.Value);
        if (performedByUserId.HasValue) query = query.Where(a => a.PerformedByUserId  == performedByUserId.Value);
        if (from.HasValue)              query = query.Where(a => a.CreatedAt          >= from.Value);
        if (to.HasValue)                query = query.Where(a => a.CreatedAt          <= to.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task AddAsync(AuditLog log, CancellationToken ct = default)
        => await _db.AuditLogs.AddAsync(log, ct);
}

using ilp_efti_connector.Domain.Enums;
using ilp_efti_connector.Infrastructure.Persistence;
using ilp_efti_connector.QueryProxyService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ilp_efti_connector.QueryProxyService.Endpoints;

public static class AuditQueryEndpoints
{
    public static WebApplication MapAuditQueryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/query/audit-logs")
            .WithTags("Query - Audit")
            .RequireAuthorization();

        // ─── GET / — lista paginata con filtri ───────────────────────────
        group.MapGet("/", async (
            EftiConnectorDbContext db,
            [FromQuery] string?   entityType        = null,
            [FromQuery] Guid?     entityId          = null,
            [FromQuery] string?   actionType        = null,
            [FromQuery] Guid?     performedByUserId = null,
            [FromQuery] DateTime? from              = null,
            [FromQuery] DateTime? to                = null,
            [FromQuery] int       page              = 1,
            [FromQuery] int       pageSize          = 20,
            CancellationToken ct = default) =>
        {
            page     = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = db.AuditLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(entityType) &&
                Enum.TryParse<AuditEntityType>(entityType, ignoreCase: true, out var parsedEntityType))
                query = query.Where(a => a.EntityType == parsedEntityType);

            if (entityId.HasValue)
                query = query.Where(a => a.EntityId == entityId.Value);

            if (!string.IsNullOrWhiteSpace(actionType) &&
                Enum.TryParse<AuditActionType>(actionType, ignoreCase: true, out var parsedActionType))
                query = query.Where(a => a.ActionType == parsedActionType);

            if (performedByUserId.HasValue)
                query = query.Where(a => a.PerformedByUserId == performedByUserId.Value);

            if (from.HasValue) query = query.Where(a => a.CreatedAt >= from.Value);
            if (to.HasValue)   query = query.Where(a => a.CreatedAt <= to.Value);

            var total      = await query.CountAsync(ct);
            var totalPages = (int)Math.Ceiling((double)total / pageSize);

            var rows = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AuditLogEntryDto(
                    a.Id,
                    a.EntityType.ToString(),
                    a.EntityId,
                    a.ActionType.ToString(),
                    a.PerformedByUserId,
                    a.PerformedBySourceId,
                    a.Description,
                    a.IpAddress,
                    a.CreatedAt))
                .ToListAsync(ct);

            return Results.Ok(new PagedResult<AuditLogEntryDto>(
                rows, total, page, pageSize, totalPages));
        })
        .WithName("GetAuditLogs")
        .WithSummary("Audit log: lista paginata con filtri per entità, azione e intervallo temporale.");

        // ─── GET /{id} — dettaglio singolo record ────────────────────────
        group.MapGet("/{id:guid}", async (
            Guid id,
            EftiConnectorDbContext db,
            CancellationToken ct) =>
        {
            var entry = await db.AuditLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id, ct);

            if (entry is null)
                return Results.NotFound(new { error = $"AuditLog {id} non trovato." });

            return Results.Ok(new AuditLogDetailDto(
                entry.Id,
                entry.EntityType.ToString(),
                entry.EntityId,
                entry.ActionType.ToString(),
                entry.PerformedByUserId,
                entry.PerformedBySourceId,
                entry.Description,
                entry.OldValueJson,
                entry.NewValueJson,
                entry.IpAddress,
                entry.UserAgent,
                entry.CreatedAt));
        })
        .WithName("GetAuditLogById")
        .WithSummary("Dettaglio singolo record di audit.");

        return app;
    }
}

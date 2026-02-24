using ilp_efti_connector.Domain.Enums;
using ilp_efti_connector.Infrastructure.Persistence;
using ilp_efti_connector.QueryProxyService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ilp_efti_connector.QueryProxyService.Endpoints;

public static class OperationQueryEndpoints
{
    public static WebApplication MapOperationQueryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/query/operations")
            .WithTags("Query - Operations")
            .RequireAuthorization();

        // ─── GET / — lista paginata con filtri ────────────────────────────
        group.MapGet("/", async (
            EftiConnectorDbContext db,
            [FromQuery] int    page       = 1,
            [FromQuery] int    pageSize   = 20,
            [FromQuery] string? status    = null,
            [FromQuery] string? code      = null,
            [FromQuery] DateTime? from    = null,
            [FromQuery] DateTime? to      = null,
            CancellationToken ct = default) =>
        {
            page     = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = db.TransportOperations
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<TransportOperationStatus>(status, ignoreCase: true, out var parsedStatus))
                query = query.Where(o => o.Status == parsedStatus);

            if (!string.IsNullOrWhiteSpace(code))
                query = query.Where(o => o.OperationCode.Contains(code));

            if (from.HasValue) query = query.Where(o => o.CreatedAt >= from.Value);
            if (to.HasValue)   query = query.Where(o => o.CreatedAt <= to.Value);

            var total      = await query.CountAsync(ct);
            var totalPages = (int)Math.Ceiling((double)total / pageSize);

            var rows = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new
                {
                    o.Id, o.OperationCode, o.DatasetType, o.Status, o.CreatedAt, o.UpdatedAt,
                    LatestMessage = o.EftiMessages
                        .OrderByDescending(m => m.CreatedAt)
                        .Select(m => new { m.Status, m.GatewayProvider, m.ExternalId, m.RetryCount })
                        .FirstOrDefault()
                })
                .ToListAsync(ct);

            var items = rows.Select(r => new OperationSummaryDto(
                Id:              r.Id,
                OperationCode:   r.OperationCode,
                DatasetType:     r.DatasetType,
                OperationStatus: r.Status.ToString(),
                MessageStatus:   r.LatestMessage?.Status.ToString(),
                GatewayProvider: r.LatestMessage?.GatewayProvider.ToString(),
                ExternalId:      r.LatestMessage?.ExternalId,
                RetryCount:      r.LatestMessage?.RetryCount ?? 0,
                CreatedAt:       r.CreatedAt,
                UpdatedAt:       r.UpdatedAt)).ToList();

            return Results.Ok(new PagedResult<OperationSummaryDto>(
                items, total, page, pageSize, totalPages));
        })
        .WithName("GetOperations")
        .WithSummary("Lista paginata operazioni con filtri opzionali.");

        // ─── GET /{id} — dettaglio con storico messaggi ───────────────────
        group.MapGet("/{id:guid}", async (
            Guid id,
            EftiConnectorDbContext db,
            CancellationToken ct) =>
        {
            var operation = await db.TransportOperations
                .AsNoTracking()
                .Include(o => o.EftiMessages)
                .FirstOrDefaultAsync(o => o.Id == id, ct);

            if (operation is null)
                return Results.NotFound(new { error = $"Operazione '{id}' non trovata." });

            var messages = operation.EftiMessages
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new MessageSummaryDto(
                    m.Id, m.GatewayProvider.ToString(), m.Status.ToString(),
                    m.ExternalId, m.ExternalUuid,
                    m.RetryCount, m.SentAt, m.AcknowledgedAt, m.NextRetryAt, m.CreatedAt))
                .ToList();

            return Results.Ok(new OperationDetailDto(
                operation.Id, operation.OperationCode, operation.DatasetType,
                operation.Status.ToString(), operation.CreatedAt, operation.UpdatedAt,
                messages));
        })
        .WithName("GetOperationDetail")
        .WithSummary("Dettaglio operazione con storico messaggi EFTI.");

        return app;
    }
}

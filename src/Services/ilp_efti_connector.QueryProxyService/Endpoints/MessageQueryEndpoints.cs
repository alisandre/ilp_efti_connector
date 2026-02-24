using ilp_efti_connector.Domain.Enums;
using ilp_efti_connector.Infrastructure.Persistence;
using ilp_efti_connector.QueryProxyService.Models;
using ilp_efti_connector.Shared.Contracts.Commands;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ilp_efti_connector.QueryProxyService.Endpoints;

public static class MessageQueryEndpoints
{
    public static WebApplication MapMessageQueryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/query/messages")
            .WithTags("Query - Messages")
            .RequireAuthorization();

        // ─── GET /dead-letter — dead letter queue paginata ────────────────
        group.MapGet("/dead-letter", async (
            EftiConnectorDbContext db,
            [FromQuery] int page     = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default) =>
        {
            page     = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var baseQuery = db.EftiMessages
                .AsNoTracking()
                .Include(m => m.TransportOperation)
                .Where(m => m.Status == MessageStatus.DEAD)
                .OrderByDescending(m => m.CreatedAt);

            var total      = await baseQuery.CountAsync(ct);
            var totalPages = (int)Math.Ceiling((double)total / pageSize);

            var rows = await baseQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var items = rows.Select(m => new DeadLetterDto(
                MessageId:            m.Id,
                TransportOperationId: m.TransportOperationId,
                OperationCode:        m.TransportOperation?.OperationCode ?? string.Empty,
                GatewayProvider:      m.GatewayProvider.ToString(),
                RetryCount:           m.RetryCount,
                CreatedAt:            m.CreatedAt)).ToList();

            return Results.Ok(new PagedResult<DeadLetterDto>(
                items, total, page, pageSize, totalPages));
        })
        .WithName("GetDeadLetterQueue")
        .WithSummary("Dead letter queue: messaggi definitivamente falliti.");

        // ─── GET /{id} — dettaglio singolo messaggio ──────────────────────
        group.MapGet("/{id:guid}", async (
            Guid id,
            EftiConnectorDbContext db,
            CancellationToken ct) =>
        {
            var message = await db.EftiMessages
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id, ct);

            if (message is null)
                return Results.NotFound(new { error = $"Messaggio '{id}' non trovato." });

            return Results.Ok(new MessageSummaryDto(
                message.Id, message.GatewayProvider.ToString(), message.Status.ToString(),
                message.ExternalId, message.ExternalUuid,
                message.RetryCount, message.SentAt, message.AcknowledgedAt,
                message.NextRetryAt, message.CreatedAt));
        })
        .WithName("GetMessageDetail")
        .WithSummary("Dettaglio singolo messaggio EFTI.");

        // ─── POST /{id}/retry — retry manuale da dashboard ────────────────
        group.MapPost("/{id:guid}/retry", async (
            Guid id,
            EftiConnectorDbContext db,
            IPublishEndpoint publish,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var message = await db.EftiMessages
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id, ct);

            if (message is null)
                return Results.NotFound(new { error = $"Messaggio '{id}' non trovato." });

            if (message.Status is not (MessageStatus.DEAD or MessageStatus.ERROR))
                return Results.BadRequest(new
                {
                    error = $"Solo i messaggi in stato DEAD o ERROR possono essere reinviati. Stato attuale: {message.Status}."
                });

            var requestedBy = user.FindFirstValue(ClaimTypes.Name)
                           ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? "operator";

            await publish.Publish(new RetryEftiMessageCommand(id, requestedBy), ct);

            return Results.Accepted("/api/query/messages/" + id, new
            {
                messageId   = id,
                requestedBy,
                status      = "RETRY_REQUESTED"
            });
        })
        .WithName("RetryMessage")
        .WithSummary("Forza il retry manuale di un messaggio in stato DEAD o ERROR.");

        return app;
    }
}

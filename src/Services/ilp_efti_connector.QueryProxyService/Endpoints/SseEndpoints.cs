using ilp_efti_connector.Domain.Enums;
using ilp_efti_connector.Domain.Interfaces.Repositories;
using ilp_efti_connector.QueryProxyService.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ilp_efti_connector.QueryProxyService.Endpoints;

public static class SseEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Stati terminali oltre i quali la stream SSE viene chiusa automaticamente.
    /// </summary>
    private static readonly HashSet<string> TerminalMessageStatuses =
        [MessageStatus.SENT.ToString(), MessageStatus.ACKNOWLEDGED.ToString(), MessageStatus.DEAD.ToString()];

    public static WebApplication MapSseEndpoints(this WebApplication app)
    {
        // ─── GET /api/query/events/{operationId} ──────────────────────────
        app.MapGet("/api/query/events/{operationId:guid}", async (
            Guid operationId,
            IServiceScopeFactory scopeFactory,
            HttpContext ctx,
            CancellationToken ct) =>
        {
            ctx.Response.Headers["Content-Type"]      = "text/event-stream";
            ctx.Response.Headers["Cache-Control"]     = "no-cache";
            ctx.Response.Headers["X-Accel-Buffering"] = "no";
            ctx.Response.Headers["Connection"]        = "keep-alive";

            // Segnala al client il retry interval in caso di disconnessione
            await WriteSseLineAsync(ctx.Response, "retry: 3000", ct);
            await ctx.Response.Body.FlushAsync(ct);

            string? lastSignature = null;

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await using var scope  = scopeFactory.CreateAsyncScope();
                    var opRepo             = scope.ServiceProvider.GetRequiredService<ITransportOperationRepository>();
                    var msgRepo            = scope.ServiceProvider.GetRequiredService<IEftiMessageRepository>();

                    var operation = await opRepo.GetByIdAsync(operationId, ct);
                    if (operation is null)
                    {
                        await WriteSseEventAsync(ctx.Response, "error",
                            new { error = $"Operazione '{operationId}' non trovata." }, ct);
                        break;
                    }

                    var messages = await msgRepo.GetByTransportOperationIdAsync(operationId, ct);
                    var latest   = messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();

                    var msgStatus = latest?.Status.ToString();
                    var signature = $"{operation.Status}|{msgStatus}|{latest?.ExternalId}|{latest?.RetryCount}";

                    if (signature != lastSignature)
                    {
                        lastSignature = signature;

                        var sseEvent = new OperationStatusSseEvent(
                            TransportOperationId: operationId,
                            OperationCode:        operation.OperationCode,
                            OperationStatus:      operation.Status.ToString(),
                            MessageStatus:        msgStatus,
                            GatewayProvider:      latest?.GatewayProvider.ToString(),
                            ExternalId:           latest?.ExternalId,
                            RetryCount:           latest?.RetryCount ?? 0,
                            SentAt:               latest?.SentAt,
                            Timestamp:            DateTime.UtcNow);

                        await WriteSseEventAsync(ctx.Response, "status", sseEvent, ct);
                    }

                    // Chiudi stream su stato terminale
                    if (msgStatus is not null && TerminalMessageStatuses.Contains(msgStatus))
                    {
                        await WriteSseEventAsync(ctx.Response, "close",
                            new { reason = "terminal_status", status = msgStatus }, ct);
                        break;
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    await WriteSseEventAsync(ctx.Response, "error",
                        new { error = ex.Message }, ct);
                    break;
                }

                await Task.Delay(2_000, ct);
            }
        })
        .WithTags("Query - SSE")
        .WithName("StreamOperationStatus")
        .WithSummary("Server-Sent Events: aggiornamenti real-time dello stato dell'operazione.")
        .RequireAuthorization();

        return app;
    }

    // ─── Helpers SSE ─────────────────────────────────────────────────────────

    private static Task WriteSseLineAsync(HttpResponse response, string line, CancellationToken ct)
        => response.WriteAsync(line + "\n", ct);

    private static async Task WriteSseEventAsync(
        HttpResponse response, string eventName, object data, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        await response.WriteAsync($"event: {eventName}\n", ct);
        await response.WriteAsync($"data: {json}\n\n", ct);
        await response.Body.FlushAsync(ct);
    }
}

using ilp_efti_connector.Infrastructure.DependencyInjection;
using ilp_efti_connector.Shared.Contracts.Dtos;
using ilp_efti_connector.Shared.Contracts.Events;
using ilp_efti_connector.Shared.Infrastructure.Extensions;
using ilp_efti_connector.Domain.Interfaces.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddIlpEftiAuth(builder.Configuration);
builder.Services.AddIlpEftiMessaging(builder.Configuration);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(opts => opts.Title = "ILP eFTI — API Gateway");
}

app.UseAuthentication();
app.UseAuthorization();

// ─── POST /api/transport-operations ──────────────────────────────────────────
app.MapPost("/api/transport-operations", async (
    [FromBody] SourcePayloadDto payload,
    [FromHeader(Name = "X-Source-Id")] Guid sourceId,
    IPublishEndpoint publish,
    CancellationToken ct) =>
{
    var operationId   = Guid.NewGuid();
    var correlationId = Guid.NewGuid().ToString();

    await publish.Publish(new TransportSubmittedEvent(
        TransportOperationId: operationId,
        SourceId:             sourceId,
        CorrelationId:        correlationId,
        RawPayloadJson:       JsonSerializer.Serialize(payload),
        DatasetType:          payload.DatasetType,
        SubmittedAt:          DateTime.UtcNow), ct);

    return Results.Accepted($"/api/transport-operations/{operationId}/status",
        new { transportOperationId = operationId, correlationId });
})
.WithName("SubmitTransportOperation")
.WithTags("TransportOperations")
.RequireAuthorization();

// ─── GET /api/transport-operations/{id}/status ───────────────────────────────
app.MapGet("/api/transport-operations/{id:guid}/status", async (
    Guid id,
    IEftiMessageRepository messages,
    ITransportOperationRepository operations,
    CancellationToken ct) =>
{
    var operation = await operations.GetByIdAsync(id, ct);
    if (operation is null)
        return Results.NotFound(new { error = $"Operazione {id} non trovata." });

    var msgList = await messages.GetByTransportOperationIdAsync(id, ct);
    var latest  = msgList.OrderByDescending(m => m.CreatedAt).FirstOrDefault();

    return Results.Ok(new
    {
        transportOperationId = id,
        operationCode        = operation.OperationCode,
        status               = latest?.Status.ToString() ?? operation.Status.ToString(),
        gatewayProvider      = latest?.GatewayProvider.ToString(),
        externalId           = latest?.ExternalId,
        retryCount           = latest?.RetryCount ?? 0,
        sentAt               = latest?.SentAt,
        acknowledgedAt       = latest?.AcknowledgedAt,
        updatedAt            = operation.UpdatedAt
    });
})
.WithName("GetTransportOperationStatus")
.WithTags("TransportOperations")
.RequireAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "ApiGateway" }))
   .AllowAnonymous();

app.Run();


var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

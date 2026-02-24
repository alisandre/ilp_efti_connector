using FluentValidation;
using ilp_efti_connector.Domain.Interfaces.Repositories;
using ilp_efti_connector.FormInputService.Models;
using ilp_efti_connector.Shared.Contracts.Dtos;
using ilp_efti_connector.Shared.Contracts.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ilp_efti_connector.FormInputService.Endpoints;

public static class TransportOperationEndpoints
{
    public static WebApplication MapTransportOperationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/forms/transport-operations")
            .WithTags("FormInput - TransportOperations")
            .RequireAuthorization();

        // ─── POST /validate — validazione inline senza pubblicare ──────────
        group.MapPost("/validate", async (
            [FromBody] SourcePayloadDto payload,
            IValidator<SourcePayloadDto> validator,
            CancellationToken ct) =>
        {
            var result = await validator.ValidateAsync(payload, ct);

            return result.IsValid
                ? Results.Ok(new FormValidationResult(true, []))
                : Results.BadRequest(new FormValidationResult(
                    IsValid: false,
                    Errors: result.Errors
                        .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage))
                        .ToList()));
        })
        .WithName("ValidateTransportOperationForm")
        .WithSummary("Valida il payload senza pubblicarlo nella pipeline.");

        // ─── POST / — valida + pubblica TransportSubmittedEvent ────────────
        group.MapPost("/", async (
            [FromBody] SourcePayloadDto payload,
            [FromHeader(Name = "X-Source-Id")] Guid sourceId,
            IValidator<SourcePayloadDto> validator,
            IPublishEndpoint publish,
            CancellationToken ct) =>
        {
            var result = await validator.ValidateAsync(payload, ct);
            if (!result.IsValid)
            {
                return Results.BadRequest(new FormValidationResult(
                    IsValid: false,
                    Errors: result.Errors
                        .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage))
                        .ToList()));
            }

            var operationId   = Guid.NewGuid();
            var correlationId = Guid.NewGuid().ToString();

            await publish.Publish(new TransportSubmittedEvent(
                TransportOperationId: operationId,
                SourceId:             sourceId,
                CorrelationId:        correlationId,
                RawPayloadJson:       JsonSerializer.Serialize(payload),
                DatasetType:          payload.DatasetType,
                SubmittedAt:          DateTime.UtcNow), ct);

            return Results.Accepted(
                $"/api/forms/transport-operations/{operationId}/status",
                new FormSubmitResponse(operationId, correlationId, "PENDING_VALIDATION"));
        })
        .WithName("SubmitTransportOperationForm")
        .WithSummary("Valida e pubblica un'operazione di trasporto nella pipeline.");

        // ─── GET /{id}/status — stato corrente per la dashboard ────────────
        group.MapGet("/{id:guid}/status", async (
            Guid id,
            ITransportOperationRepository operations,
            IEftiMessageRepository messages,
            CancellationToken ct) =>
        {
            var operation = await operations.GetByIdAsync(id, ct);
            if (operation is null)
                return Results.NotFound(new { error = $"Operazione '{id}' non trovata." });

            var msgList = await messages.GetByTransportOperationIdAsync(id, ct);
            var latest  = msgList.OrderByDescending(m => m.CreatedAt).FirstOrDefault();

            return Results.Ok(new FormOperationStatusResponse(
                TransportOperationId: id,
                OperationCode:        operation.OperationCode,
                DatasetType:          operation.DatasetType,
                Status:               latest?.Status.ToString() ?? operation.Status.ToString(),
                GatewayProvider:      latest?.GatewayProvider.ToString(),
                ExternalId:           latest?.ExternalId,
                RetryCount:           latest?.RetryCount ?? 0,
                SentAt:               latest?.SentAt,
                AcknowledgedAt:       latest?.AcknowledgedAt,
                CreatedAt:            operation.CreatedAt,
                UpdatedAt:            operation.UpdatedAt));
        })
        .WithName("GetFormOperationStatus")
        .WithSummary("Restituisce lo stato aggiornato di un'operazione.");

        return app;
    }
}

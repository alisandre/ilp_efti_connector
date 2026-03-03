using ilp_efti_connector.Domain.Interfaces.Repositories;
using ilp_efti_connector.Shared.Contracts.Dtos;
using ilp_efti_connector.Shared.Contracts.Events;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ilp_efti_connector.ApiGateway.Controllers;

[ApiController]
[Route("api/transport-operations")]
[Authorize]
public class TransportOperationsController : ControllerBase
{
    private readonly IPublishEndpoint _publish;
    private readonly ITransportOperationRepository _operations;
    private readonly IEftiMessageRepository _messages;

    public TransportOperationsController(
        IPublishEndpoint publish,
        ITransportOperationRepository operations,
        IEftiMessageRepository messages)
    {
        _publish    = publish;
        _operations = operations;
        _messages   = messages;
    }

    // POST /api/transport-operations
    [HttpPost]
    [ProducesResponseType(typeof(TransportOperationSubmitResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTransportOperation(
        [FromBody] SourcePayloadDto payload,
        [FromHeader(Name = "X-Source-Id")] Guid sourceId,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem();

        var operationId   = Guid.NewGuid();
        var correlationId = Guid.NewGuid().ToString();

        await _publish.Publish(new TransportSubmittedEvent(
            TransportOperationId: operationId,
            SourceId:             sourceId,
            CorrelationId:        correlationId,
            RawPayloadJson:       JsonSerializer.Serialize(payload),
            DatasetType:          payload.DatasetType,
            SubmittedAt:          DateTime.UtcNow), ct);

        var response = new TransportOperationSubmitResponse(operationId, correlationId, "PENDING_VALIDATION");

        return AcceptedAtAction(nameof(GetTransportOperationStatus), new { id = operationId }, response);
    }

    // GET /api/transport-operations/{id}/status
    [HttpGet("{id:guid}/status")]
    [ProducesResponseType(typeof(TransportOperationStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransportOperationStatus(Guid id, CancellationToken ct)
    {
        var operation = await _operations.GetByIdAsync(id, ct);
        if (operation is null)
            return NotFound(new { error = $"Operazione '{id}' non trovata." });

        var messages = await _messages.GetByTransportOperationIdAsync(id, ct);
        var latest   = messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();

        return Ok(new TransportOperationStatusResponse(
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
    }
}

public sealed record TransportOperationSubmitResponse(
    Guid   TransportOperationId,
    string CorrelationId,
    string Status);

public sealed record TransportOperationStatusResponse(
    Guid      TransportOperationId,
    string    OperationCode,
    string    DatasetType,
    string    Status,
    string?   GatewayProvider,
    string?   ExternalId,
    short     RetryCount,
    DateTime? SentAt,
    DateTime? AcknowledgedAt,
    DateTime  CreatedAt,
    DateTime  UpdatedAt);

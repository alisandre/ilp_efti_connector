using ilp_efti_connector.Shared.Contracts.Dtos;
using ilp_efti_connector.Shared.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ilp_efti_connector.ValidationService.Consumers;

public sealed class TransportSubmittedConsumer : IConsumer<TransportSubmittedEvent>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IPublishEndpoint _publish;
    private readonly ILogger<TransportSubmittedConsumer> _logger;

    public TransportSubmittedConsumer(IPublishEndpoint publish, ILogger<TransportSubmittedConsumer> logger)
    {
        _publish = publish;
        _logger  = logger;
    }

    public async Task Consume(ConsumeContext<TransportSubmittedEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation("Validazione payload → OperationId={Id} DatasetType={Type}",
            evt.TransportOperationId, evt.DatasetType);

        var errors = Validate(evt);

        if (errors.Count > 0)
        {
            _logger.LogWarning("Validazione fallita per {Id}: {Errors}",
                evt.TransportOperationId, string.Join("; ", errors));

            await _publish.Publish(new TransportValidationFailedEvent(
                TransportOperationId: evt.TransportOperationId,
                SourceId:             evt.SourceId,
                CorrelationId:        evt.CorrelationId,
                DatasetType:          evt.DatasetType,
                ValidationErrors:     errors,
                FailedAt:             DateTime.UtcNow), context.CancellationToken);

            return;
        }

        await _publish.Publish(new TransportValidatedEvent(
            TransportOperationId: evt.TransportOperationId,
            SourceId:             evt.SourceId,
            CorrelationId:        evt.CorrelationId,
            RawPayloadJson:       evt.RawPayloadJson,
            DatasetType:          evt.DatasetType,
            ValidatedAt:          DateTime.UtcNow), context.CancellationToken);

        _logger.LogInformation("Payload validato → OperationId={Id}", evt.TransportOperationId);
    }

    private static IReadOnlyList<string> Validate(TransportSubmittedEvent evt)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(evt.RawPayloadJson))
        {
            errors.Add("RawPayloadJson è obbligatorio.");
            return errors;
        }

        SourcePayloadDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<SourcePayloadDto>(evt.RawPayloadJson, JsonOptions);
        }
        catch (JsonException ex)
        {
            errors.Add($"RawPayloadJson non è un JSON valido: {ex.Message}");
            return errors;
        }

        if (dto is null)
        {
            errors.Add("Payload deserializzato come null.");
            return errors;
        }

        if (string.IsNullOrWhiteSpace(dto.OperationCode))    errors.Add("OperationCode è obbligatorio.");
        if (string.IsNullOrWhiteSpace(dto.DatasetType))      errors.Add("DatasetType è obbligatorio.");
        if (string.IsNullOrWhiteSpace(dto.CustomerCode))     errors.Add("CustomerCode è obbligatorio.");
        if (string.IsNullOrWhiteSpace(dto.CustomerName))     errors.Add("CustomerName è obbligatorio.");
        if (dto.Consignee is null)                            errors.Add("Consignee è obbligatorio.");
        if (dto.Carriers is null || dto.Carriers.Count == 0) errors.Add("Almeno un vettore è obbligatorio.");

        return errors;
    }
}

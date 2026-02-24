namespace ilp_efti_connector.Shared.Contracts.Events;

/// <summary>
/// Pubblicato dall'API Gateway quando un'operazione di trasporto è stata ricevuta e salvata.
/// Consumed by: ValidationService
/// </summary>
public record TransportSubmittedEvent(
    Guid     TransportOperationId,
    Guid     SourceId,
    string   CorrelationId,
    string   RawPayloadJson,
    string   DatasetType,
    DateTime SubmittedAt
);

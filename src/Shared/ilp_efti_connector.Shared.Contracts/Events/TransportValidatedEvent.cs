namespace ilp_efti_connector.Shared.Contracts.Events;

/// <summary>
/// Pubblicato dal ValidationService quando il payload supera la validazione.
/// Consumed by: NormalizationService
/// </summary>
public record TransportValidatedEvent(
    Guid     TransportOperationId,
    Guid     SourceId,
    string   CorrelationId,
    string   RawPayloadJson,
    string   DatasetType,
    DateTime ValidatedAt
);

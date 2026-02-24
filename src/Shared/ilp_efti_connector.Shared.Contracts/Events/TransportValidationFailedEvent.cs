namespace ilp_efti_connector.Shared.Contracts.Events;

/// <summary>
/// Pubblicato dal ValidationService quando il payload non supera la validazione.
/// Consumed by: NotificationService (per notificare la sorgente del rifiuto)
/// </summary>
public record TransportValidationFailedEvent(
    Guid                 TransportOperationId,
    Guid                 SourceId,
    string               CorrelationId,
    string               DatasetType,
    IReadOnlyList<string> ValidationErrors,
    DateTime             FailedAt
);

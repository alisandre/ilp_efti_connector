namespace ilp_efti_connector.Shared.Contracts.Events;

/// <summary>
/// Pubblicato dal ResponseHandlerService quando la sorgente deve essere notificata dell'esito.
/// Consumed by: NotificationService
/// </summary>
public record SourceNotificationRequiredEvent(
    Guid     TransportOperationId,
    Guid     SourceId,
    string   CorrelationId,
    string   Status,
    string?  ExternalId,
    string?  ExternalUuid,
    string?  ErrorMessage,
    DateTime OccurredAt
);

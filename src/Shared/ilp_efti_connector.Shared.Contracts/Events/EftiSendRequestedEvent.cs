namespace ilp_efti_connector.Shared.Contracts.Events;

/// <summary>
/// Pubblicato dal NormalizationService quando l'EftiMessage è stato creato ed è pronto per l'invio.
/// Consumed by: EftiGatewayService
/// </summary>
public record EftiSendRequestedEvent(
    Guid     EftiMessageId,
    Guid     TransportOperationId,
    string   CorrelationId,
    string   GatewayProvider,
    string   PayloadJson,
    string   DatasetType
);

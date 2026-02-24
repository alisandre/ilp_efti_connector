namespace ilp_efti_connector.Shared.Contracts.Events;

/// <summary>
/// Pubblicato dall'EftiGatewayService dopo aver ricevuto la risposta dal gateway (MILOS o EFTI Native).
/// Consumed by: ResponseHandlerService
/// </summary>
public record EftiResponseReceivedEvent(
    Guid     EftiMessageId,
    Guid     TransportOperationId,
    string   CorrelationId,
    string   GatewayProvider,
    bool     IsSuccess,
    string?  ExternalId,
    string?  ExternalUuid,
    string?  ErrorMessage,
    DateTime ReceivedAt
);

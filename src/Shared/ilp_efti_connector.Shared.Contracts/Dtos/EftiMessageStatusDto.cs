namespace ilp_efti_connector.Shared.Contracts.Dtos;

/// <summary>
/// DTO di stato di un messaggio EFTI. Usato nelle notifiche webhook verso le sorgenti
/// e nelle risposte SSE alla dashboard React.
/// </summary>
public record EftiMessageStatusDto(
    Guid     EftiMessageId,
    Guid     TransportOperationId,
    string   OperationCode,
    string   Status,
    string   GatewayProvider,
    string?  ExternalId,
    string?  ExternalUuid,
    short    RetryCount,
    DateTime? SentAt,
    DateTime? AcknowledgedAt,
    string?  ErrorMessage,
    DateTime UpdatedAt
);

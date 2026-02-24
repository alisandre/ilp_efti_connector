namespace ilp_efti_connector.QueryProxyService.Models;

/// <summary>Riepilogo operazione per la lista dashboard.</summary>
public sealed record OperationSummaryDto(
    Guid      Id,
    string    OperationCode,
    string    DatasetType,
    string    OperationStatus,
    string?   MessageStatus,
    string?   GatewayProvider,
    string?   ExternalId,
    short     RetryCount,
    DateTime  CreatedAt,
    DateTime  UpdatedAt
);

/// <summary>Dettaglio completo operazione con storico messaggi.</summary>
public sealed record OperationDetailDto(
    Guid                          Id,
    string                        OperationCode,
    string                        DatasetType,
    string                        Status,
    DateTime                      CreatedAt,
    DateTime                      UpdatedAt,
    IReadOnlyList<MessageSummaryDto> Messages
);

/// <summary>Riepilogo singolo messaggio EFTI.</summary>
public sealed record MessageSummaryDto(
    Guid      Id,
    string    GatewayProvider,
    string    Status,
    string?   ExternalId,
    string?   ExternalUuid,
    short     RetryCount,
    DateTime? SentAt,
    DateTime? AcknowledgedAt,
    DateTime? NextRetryAt,
    DateTime  CreatedAt
);

/// <summary>Messaggio nella dead letter queue.</summary>
public sealed record DeadLetterDto(
    Guid     MessageId,
    Guid     TransportOperationId,
    string   OperationCode,
    string   GatewayProvider,
    short    RetryCount,
    DateTime CreatedAt
);

/// <summary>Risultato paginato generico.</summary>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int              TotalCount,
    int              Page,
    int              PageSize,
    int              TotalPages
);

/// <summary>Payload dell'evento SSE inviato al client quando lo stato cambia.</summary>
public sealed record OperationStatusSseEvent(
    Guid      TransportOperationId,
    string    OperationCode,
    string    OperationStatus,
    string?   MessageStatus,
    string?   GatewayProvider,
    string?   ExternalId,
    short     RetryCount,
    DateTime? SentAt,
    DateTime  Timestamp
);

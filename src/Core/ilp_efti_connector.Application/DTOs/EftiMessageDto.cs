using ilp_efti_connector.Domain.Enums;

namespace ilp_efti_connector.Application.DTOs;

public sealed record EftiMessageDto(
    Guid Id,
    Guid SourceId,
    Guid TransportOperationId,
    Guid CorrelationId,
    GatewayProvider GatewayProvider,
    MessageDirection Direction,
    string DatasetType,
    MessageStatus Status,
    string? ExternalId,
    string? ExternalUuid,
    short RetryCount,
    DateTime? NextRetryAt,
    DateTime? SentAt,
    DateTime? AcknowledgedAt,
    DateTime CreatedAt
);

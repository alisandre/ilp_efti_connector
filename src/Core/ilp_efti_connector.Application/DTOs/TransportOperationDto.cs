using ilp_efti_connector.Domain.Enums;

namespace ilp_efti_connector.Application.DTOs;

public sealed record TransportOperationDto(
    Guid Id,
    Guid SourceId,
    Guid CustomerId,
    Guid? DestinationId,
    string OperationCode,
    string DatasetType,
    TransportOperationStatus Status,
    string? Hashcode,
    string? HashcodeAlgorithm,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid? CreatedByUserId
);

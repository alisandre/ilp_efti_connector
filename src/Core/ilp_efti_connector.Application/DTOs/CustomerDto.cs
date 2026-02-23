namespace ilp_efti_connector.Application.DTOs;

public sealed record CustomerDto(
    Guid Id,
    string CustomerCode,
    string BusinessName,
    string? VatNumber,
    string? EoriCode,
    string? ContactEmail,
    bool IsActive,
    bool AutoCreated,
    Guid? SourceId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

namespace ilp_efti_connector.Application.DTOs;

public sealed record CustomerDestinationDto(
    Guid Id,
    Guid CustomerId,
    string DestinationCode,
    string? Label,
    string AddressLine1,
    string City,
    string? PostalCode,
    string? Province,
    string CountryCode,
    string? UnLocode,
    bool IsDefault,
    bool AutoCreated,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

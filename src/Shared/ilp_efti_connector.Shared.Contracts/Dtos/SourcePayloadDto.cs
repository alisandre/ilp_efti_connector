namespace ilp_efti_connector.Shared.Contracts.Dtos;

/// <summary>
/// DTO del payload grezzo inviato da un sistema sorgente all'API Gateway.
/// Viene serializzato in JSON e trasportato nel bus tramite TransportSubmittedEvent.
/// </summary>
public record SourcePayloadDto(
    string         OperationCode,
    string         DatasetType,
    string         CustomerCode,
    string         CustomerName,
    string?        CustomerVat,
    string?        CustomerEori,
    string?        DestinationCode,
    ConsignorAddressDto?          ConsignorAddress,
    ConsigneeDto?                 Consignee,
    IReadOnlyList<CarrierDto>     Carriers,
    AcceptanceLocationDto?        AcceptanceLocation,
    DeliveryLocationDto?          DeliveryLocation,
    ConsignmentItemsDto?          ConsignmentItems,
    TransportDetailsDto?          TransportDetails,
    HashcodeDto?                  Hashcode
);

public record ConsignorAddressDto(
    string? StreetName,
    string? PostCode,
    string  CityName,
    string  CountryCode,
    string? CountryName
);

public record ConsigneeDto(
    string  Name,
    string  PlayerType,
    string? TaxRegistration,
    string? EoriCode,
    string? StreetName,
    string? PostCode,
    string  CityName,
    string  CountryCode,
    string? CountryName
);

public record CarrierDto(
    int     SortOrder,
    string  Name,
    string  PlayerType,
    string  TractorPlate,
    string? TaxRegistration,
    string? EoriCode,
    string? EquipmentCategory,
    string? StreetName,
    string? PostCode,
    string  CityName,
    string  CountryCode,
    string? CountryName
);

public record AcceptanceLocationDto(
    string? StreetName,
    string? PostCode,
    string  CityName,
    string  CountryCode,
    string? CountryName,
    DateTime? Date
);

public record DeliveryLocationDto(
    string? StreetName,
    string? PostCode,
    string  CityName,
    string  CountryCode,
    string? CountryName
);

public record ConsignmentItemsDto(
    int      TotalItemQuantity,
    decimal  TotalWeight,
    decimal? TotalVolume,
    IReadOnlyList<PackageDto> Packages
);

public record PackageDto(
    int      SortOrder,
    string?  ShippingMarks,
    int      ItemQuantity,
    string?  TypeCode,
    decimal  GrossWeight,
    decimal? GrossVolume
);

public record TransportDetailsDto(
    string? CargoType,
    string? Incoterms
);

public record HashcodeDto(
    string Value,
    string Algorithm
);

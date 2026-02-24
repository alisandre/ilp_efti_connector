using ilp_efti_connector.Domain.Enums;
using MediatR;

namespace ilp_efti_connector.Application.TransportOperations.Commands.SubmitTransportOperation;

/// <summary>
/// Crea una nuova TransportOperation con tutte le entità figlio e l'EftiMessage iniziale.
/// Usato dal NormalizationService dopo l'upsert cliente/destinazione.
/// </summary>
public sealed record SubmitTransportOperationCommand(
    Guid   SourceId,
    Guid   CustomerId,
    Guid?  DestinationId,
    string OperationCode,
    string DatasetType,
    // ID pre-assegnato dall'ApiGateway (se null viene generato nel handler)
    Guid?  TransportOperationId,
    // Consignee
    ConsigneeData?          Consignee,
    // Carriers (preservano l'ordine)
    IReadOnlyList<CarrierData> Carriers,
    // Dettagli trasporto
    TransportDetailData?    Detail,
    // Spedizione
    ConsignmentData?        ConsignmentItem,
    // Snapshot JSON per debug
    string? RawPayloadJson,
    // Provider gateway corrente (determina quale gateway invia)
    GatewayProvider GatewayProvider
) : IRequest<SubmitTransportOperationResult>;

/// <summary>Risultato del comando: IDs di operation e messaggio creati.</summary>
public sealed record SubmitTransportOperationResult(
    Guid TransportOperationId,
    Guid EftiMessageId
);

public sealed record ConsigneeData(
    string  Name,
    PlayerType PlayerType,
    string? TaxRegistration,
    string? EoriCode,
    string? StreetName,
    string? PostCode,
    string  CityName,
    string  CountryCode,
    string? CountryName
);

public sealed record CarrierData(
    int     SortOrder,
    string  Name,
    PlayerType PlayerType,
    string  TractorPlate,
    string? TaxRegistration,
    string? EoriCode,
    EcmrEquipmentCategory? EquipmentCategory,
    string? StreetName,
    string? PostCode,
    string  CityName,
    string  CountryCode,
    string? CountryName
);

public sealed record TransportDetailData(
    CargoType? CargoType,
    Incoterms? Incoterms,
    string? AcceptanceStreetName,
    string? AcceptancePostCode,
    string? AcceptanceCityName,
    string? AcceptanceCountryCode,
    string? AcceptanceCountryName,
    DateTime? AcceptanceDate,
    string? ReceiptStreetName,
    string? ReceiptPostCode,
    string? ReceiptCityName,
    string? ReceiptCountryCode,
    string? ReceiptCountryName
);

public sealed record ConsignmentData(
    int     TotalItemQuantity,
    decimal TotalWeight,
    decimal? TotalVolume,
    IReadOnlyList<PackageData> Packages
);

public sealed record PackageData(
    int     SortOrder,
    string? ShippingMarks,
    int     ItemQuantity,
    string? TypeCode,
    decimal GrossWeight,
    decimal? GrossVolume
);

using ilp_efti_connector.Domain.Enums;

namespace ilp_efti_connector.Gateway.Contracts.Models;

/// <summary>
/// Modello interno normalizzato del documento di trasporto.
/// Indipendente dal provider: ogni gateway lo mappa nel proprio formato
/// (ECMRRequest MILOS in Fase 1 | Dataset EN 17532 in Fase 2).
/// </summary>
public record EcmrPayload(
    string                     OperationCode,
    string                     DatasetType,
    bool                       IsMasterCmr,
    string?                    Note,
    ConsignorInfo              Consignor,
    ConsigneeInfo              Consignee,
    IReadOnlyList<CarrierInfo> Carriers,
    AcceptanceLocation         PickupLocation,
    DeliveryLocation           DeliveryLocation,
    ConsignmentItems           Goods,
    TransportDetailsInfo       TransportDetails,
    HashcodeInfo?              Hashcode
);

/// <summary>Mittente / consignor dell'operazione di trasporto.</summary>
public record ConsignorInfo(
    string      Name,
    string?     VatNumber,
    string?     EoriCode,
    AddressInfo Address);

/// <summary>Destinatario / consignee dell'operazione di trasporto.</summary>
public record ConsigneeInfo(
    string      Name,
    PlayerType  PlayerType,
    string?     TaxRegistration,
    string?     EoriCode,
    AddressInfo Address
);

/// <summary>Vettore. Un'operazione può avere più vettori ordinati per <see cref="SortOrder"/>.</summary>
public record CarrierInfo(
    int                    SortOrder,
    string                 Name,
    PlayerType             PlayerType,
    string?                TaxRegistration,
    string?                EoriCode,
    string?                TractorPlate,
    string?                TrailerPlate,
    string?                TractorPlateCountryCode,
    string?                TrailerPlateCountryCode,
    EcmrEquipmentCategory? EquipmentCategory,
    AddressInfo            Address
);

/// <summary>Indirizzo postale generico usato per mittente, destinatario, vettore e luoghi.</summary>
public record AddressInfo(
    string  Street,
    string  City,
    string? PostalCode,
    string  CountryCode,
    string? CountryName,
    string? Province
);

/// <summary>Luogo e data di presa in carico della merce (contractualCarrierAcceptanceLocation).</summary>
public record AcceptanceLocation(
    AddressInfo Address,
    DateTime?   Date
);

/// <summary>Luogo di consegna della merce (contractualConsigneeReceiptLocation).</summary>
public record DeliveryLocation(
    AddressInfo Address
);

/// <summary>Dettaglio merce spedita: colli, peso, volume.</summary>
public record ConsignmentItems(
    int                           TotalItemQuantity,
    decimal                       TotalWeight,
    decimal?                      TotalVolume,
    IReadOnlyList<ConsignmentPackage> Packages
);

/// <summary>Singolo collo / pallet nella spedizione.</summary>
public record ConsignmentPackage(
    int      SortOrder,
    string?  ShippingMarks,
    int      ItemQuantity,
    string?  TypeCode,
    decimal  GrossWeight,
    decimal? GrossVolume
);

/// <summary>Dettagli commerciali del trasporto.</summary>
public record TransportDetailsInfo(
    CargoType? CargoType,
    Incoterms? Incoterms
);

/// <summary>Hash di integrità del payload (SHA-256).</summary>
public record HashcodeInfo(
    string Value,
    string Algorithm
);

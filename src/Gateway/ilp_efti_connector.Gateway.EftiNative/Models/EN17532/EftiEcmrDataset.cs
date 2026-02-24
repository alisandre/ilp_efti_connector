using System.Text.Json.Serialization;

namespace ilp_efti_connector.Gateway.EftiNative.Models.EN17532;

/// <summary>
/// Dataset eFTI principale secondo lo standard EN 17532.
/// Inviato al gateway EFTI Gate nazionale tramite POST /datasets.
/// </summary>
public sealed class EftiEcmrDataset
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Tipo documento: ECMR | EDDT | eAWB | eBL | eRSD | eDAD</summary>
    [JsonPropertyName("typeCode")]
    public string TypeCode { get; set; } = string.Empty;

    /// <summary>Data/ora emissione ISO 8601. Es: 2026-03-01T08:00:00Z</summary>
    [JsonPropertyName("issueDateTime")]
    public string IssueDateTime { get; set; } = string.Empty;

    [JsonPropertyName("consignor")]
    public EftiConsignor Consignor { get; set; } = new();

    [JsonPropertyName("consignee")]
    public EftiConsignee Consignee { get; set; } = new();

    [JsonPropertyName("carriers")]
    public List<EftiCarrier> Carriers { get; set; } = [];

    [JsonPropertyName("acceptanceLocation")]
    public EftiLocation? AcceptanceLocation { get; set; }

    [JsonPropertyName("deliveryLocation")]
    public EftiLocation? DeliveryLocation { get; set; }

    [JsonPropertyName("consignmentItems")]
    public EftiGoods? ConsignmentItems { get; set; }

    [JsonPropertyName("transportDetails")]
    public EftiTransportDetails? TransportDetails { get; set; }

    [JsonPropertyName("hashcode")]
    public EftiHashcode? Hashcode { get; set; }
}

/// <summary>Risposta al submit del dataset (202 Accepted).</summary>
public sealed class EftiSubmitResponse
{
    [JsonPropertyName("messageId")]
    public string MessageId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

/// <summary>Indirizzo postale EN 17532.</summary>
public sealed class EftiAddress
{
    [JsonPropertyName("streetName")]
    public string? StreetName { get; set; }

    [JsonPropertyName("postCode")]
    public string? PostCode { get; set; }

    [JsonPropertyName("cityName")]
    public string CityName { get; set; } = string.Empty;

    [JsonPropertyName("countryCode")]
    public string CountryCode { get; set; } = string.Empty;

    [JsonPropertyName("countryName")]
    public string? CountryName { get; set; }
}

/// <summary>Luogo con indirizzo e data/ora opzionale.</summary>
public sealed class EftiLocation
{
    [JsonPropertyName("address")]
    public EftiAddress? Address { get; set; }

    [JsonPropertyName("dateTime")]
    public string? DateTime { get; set; }
}

/// <summary>Dettagli commerciali del trasporto.</summary>
public sealed class EftiTransportDetails
{
    [JsonPropertyName("cargoType")]
    public string? CargoType { get; set; }

    [JsonPropertyName("incoterms")]
    public string? Incoterms { get; set; }
}

/// <summary>Hash di integrità del dataset (SHA-256).</summary>
public sealed class EftiHashcode
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("algorithm")]
    public string Algorithm { get; set; } = "SHA-256";
}

/// <summary>Vettore coinvolto nel trasporto EN 17532.</summary>
public sealed class EftiCarrier
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("playerType")]
    public string? PlayerType { get; set; }

    [JsonPropertyName("taxId")]
    public string? TaxId { get; set; }

    [JsonPropertyName("eoriCode")]
    public string? EoriCode { get; set; }

    [JsonPropertyName("tractorPlate")]
    public string? TractorPlate { get; set; }

    [JsonPropertyName("trailerPlate")]
    public string? TrailerPlate { get; set; }

    [JsonPropertyName("tractorPlateCountryCode")]
    public string? TractorPlateCountryCode { get; set; }

    [JsonPropertyName("trailerPlateCountryCode")]
    public string? TrailerPlateCountryCode { get; set; }

    [JsonPropertyName("equipmentCategory")]
    public string? EquipmentCategory { get; set; }

    [JsonPropertyName("address")]
    public EftiAddress? Address { get; set; }
}

/// <summary>Merce trasportata (consignment items) EN 17532.</summary>
public sealed class EftiGoods
{
    [JsonPropertyName("totalItemQuantity")]
    public int TotalItemQuantity { get; set; }

    [JsonPropertyName("totalWeight")]
    public decimal TotalWeight { get; set; }

    [JsonPropertyName("totalVolume")]
    public decimal? TotalVolume { get; set; }

    [JsonPropertyName("packages")]
    public List<EftiPackage> Packages { get; set; } = [];
}

/// <summary>Singolo collo / pallet EN 17532.</summary>
public sealed class EftiPackage
{
    [JsonPropertyName("shippingMarks")]
    public string? ShippingMarks { get; set; }

    [JsonPropertyName("itemQuantity")]
    public int ItemQuantity { get; set; }

    [JsonPropertyName("typeCode")]
    public string? TypeCode { get; set; }

    [JsonPropertyName("grossWeight")]
    public decimal GrossWeight { get; set; }

    [JsonPropertyName("grossVolume")]
    public decimal? GrossVolume { get; set; }
}

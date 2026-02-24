using ilp_efti_connector.Domain.Enums;
using ilp_efti_connector.Gateway.Contracts.Models;
using ilp_efti_connector.Gateway.Milos.Models;

namespace ilp_efti_connector.Gateway.Milos.Mapping;

/// <summary>
/// Converte il modello interno <see cref="EcmrPayload"/> (Gateway.Contracts)
/// nel formato MILOS <see cref="ECMRRequest"/> (ICD Circle SpA v1.0).
/// </summary>
public static class EcmrPayloadToMilosMapper
{
    public static ECMRRequest Map(EcmrPayload payload)
    {
        return new ECMRRequest
        {
            Shipping = new Shipping
            {
                ECMRId      = payload.OperationCode,
                DatasetType = payload.DatasetType
            },

            ConsignorSender = new Player
            {
                Name            = payload.Consignor.Name,
                Type            = "CONSIGNOR_SENDER",
                TaxRegistration = payload.Consignor.VatNumber,
                EORICode        = payload.Consignor.EoriCode,
                PostalAddress   = MapAddress(payload.Consignor.Address)
            },

            Consignee = new Player
            {
                Name            = payload.Consignee.Name,
                Type            = payload.Consignee.PlayerType.ToString(),
                TaxRegistration = payload.Consignee.TaxRegistration,
                EORICode        = payload.Consignee.EoriCode,
                PostalAddress   = MapAddress(payload.Consignee.Address)
            },

            Carriers = payload.Carriers
                .OrderBy(c => c.SortOrder)
                .Select(MapCarrier)
                .ToList(),

            ContractualCarrierAcceptanceLocation = new ContractualCarrierAcceptanceLocation
            {
                PostalAddress = MapAddress(payload.PickupLocation.Address),
                Date          = payload.PickupLocation.Date?.ToString("yyyy-MM-dd HH:mm:ss")
            },

            ContractualConsigneeReceiptLocation = new ContractualConsigneeReceiptLocation
            {
                PostalAddress = MapAddress(payload.DeliveryLocation.Address)
            },

            IncludedConsignmentItems = MapGoods(payload.Goods),

            TransportDetails = payload.TransportDetails.CargoType.HasValue || payload.TransportDetails.Incoterms.HasValue
                ? new MilosTransportDetails
                  {
                      CargoType = MapCargoType(payload.TransportDetails.CargoType),
                      Incoterms = payload.TransportDetails.Incoterms?.ToString()
                  }
                : null
        };
    }

    private static Carrier MapCarrier(CarrierInfo c) => new()
    {
        Name                     = c.Name,
        Type                     = c.PlayerType.ToString(),
        TaxRegistration          = c.TaxRegistration,
        EORICode                 = c.EoriCode,
        TractorPlate             = c.TractorPlate,
        TrailerPlate             = c.TrailerPlate,
        TractorPlateCountryCode  = c.TractorPlateCountryCode,
        TrailerPlateCountryCode  = c.TrailerPlateCountryCode,
        EquipmentCategory        = c.EquipmentCategory?.ToString(),
        PostalAddress            = MapAddress(c.Address)
    };

    private static PostalAddress MapAddress(AddressInfo a) => new()
    {
        StreetName  = a.Street,
        PostCode    = a.PostalCode,
        CityName    = a.City,
        CountryCode = a.CountryCode,
        CountryName = a.CountryName
    };

    private static IncludedConsignmentItems MapGoods(ConsignmentItems goods) => new()
    {
        TotalItemQuantity = goods.TotalItemQuantity,
        TotalWeight       = goods.TotalWeight,
        TotalVolume       = goods.TotalVolume,
        TransportPackages = goods.Packages
            .OrderBy(p => p.SortOrder)
            .Select(p => new MilosTransportPackage
            {
                ShippingMarks = p.ShippingMarks,
                ItemQuantity  = p.ItemQuantity,
                TypeCode      = p.TypeCode,
                GrossWeight   = p.GrossWeight,
                GrossVolume   = p.GrossVolume
            })
            .ToList()
    };

    /// <summary>
    /// Converte la risposta MILOS <see cref="ECMRRequest"/> (dal GET) nel modello interno <see cref="EcmrPayload"/>.
    /// </summary>
    public static EcmrPayload MapBack(ECMRRequest req)
    {
        return new EcmrPayload(
            OperationCode:    req.Shipping.ECMRId,
            DatasetType:      req.Shipping.DatasetType,
            IsMasterCmr:      false,
            Note:             null,
            Consignor: new ConsignorInfo(
                Name:      req.ConsignorSender.Name,
                VatNumber: req.ConsignorSender.TaxRegistration,
                EoriCode:  req.ConsignorSender.EORICode,
                Address:   MapAddressBack(req.ConsignorSender.PostalAddress)),
            Consignee: new ConsigneeInfo(
                Name:            req.Consignee.Name,
                PlayerType:      ParsePlayerType(req.Consignee.Type),
                TaxRegistration: req.Consignee.TaxRegistration,
                EoriCode:        req.Consignee.EORICode,
                Address:         MapAddressBack(req.Consignee.PostalAddress)),
            Carriers: req.Carriers.Select((c, i) => new CarrierInfo(
                SortOrder:               i,
                Name:                    c.Name,
                PlayerType:              ParsePlayerType(c.Type),
                TaxRegistration:         c.TaxRegistration,
                EoriCode:                c.EORICode,
                TractorPlate:            c.TractorPlate,
                TrailerPlate:            c.TrailerPlate,
                TractorPlateCountryCode: c.TractorPlateCountryCode,
                TrailerPlateCountryCode: c.TrailerPlateCountryCode,
                EquipmentCategory:       ParseEquipmentCategory(c.EquipmentCategory),
                Address:                 MapAddressBack(c.PostalAddress))).ToList(),
            PickupLocation: new AcceptanceLocation(
                Address: MapAddressBack(req.ContractualCarrierAcceptanceLocation?.PostalAddress),
                Date:    ParseDate(req.ContractualCarrierAcceptanceLocation?.Date)),
            DeliveryLocation: new DeliveryLocation(
                Address: MapAddressBack(req.ContractualConsigneeReceiptLocation?.PostalAddress)),
            Goods: req.IncludedConsignmentItems is { } g
                ? new ConsignmentItems(
                    TotalItemQuantity: g.TotalItemQuantity,
                    TotalWeight:       g.TotalWeight,
                    TotalVolume:       g.TotalVolume,
                    Packages:          g.TransportPackages.Select((p, i) => new ConsignmentPackage(
                        SortOrder:    i,
                        ShippingMarks: p.ShippingMarks,
                        ItemQuantity:  p.ItemQuantity,
                        TypeCode:      p.TypeCode,
                        GrossWeight:   p.GrossWeight,
                        GrossVolume:   p.GrossVolume)).ToList())
                : new ConsignmentItems(0, 0, null, []),
            TransportDetails: new TransportDetailsInfo(
                CargoType: ParseCargoType(req.TransportDetails?.CargoType),
                Incoterms: ParseIncoterms(req.TransportDetails?.Incoterms)),
            Hashcode: null
        );
    }

    private static AddressInfo MapAddressBack(PostalAddress? a) => a is null
        ? new AddressInfo(string.Empty, string.Empty, null, string.Empty, null, null)
        : new AddressInfo(a.StreetName ?? string.Empty, a.CityName, a.PostCode, a.CountryCode, a.CountryName, null);

    private static PlayerType ParsePlayerType(string? value) =>
        Enum.TryParse<PlayerType>(value, ignoreCase: true, out var result) ? result : PlayerType.CARRIER;

    private static EcmrEquipmentCategory? ParseEquipmentCategory(string? value) =>
        Enum.TryParse<EcmrEquipmentCategory>(value, ignoreCase: true, out var result) ? result : null;

    private static CargoType? ParseCargoType(string? value) => value?.ToUpperInvariant() switch
    {
        "FTL"      => CargoType.FTL,
        "LTL"      => CargoType.LTL,
        "GROUPAGE" => CargoType.GROUPAGE,
        _          => null
    };

    private static Incoterms? ParseIncoterms(string? value) =>
        Enum.TryParse<Incoterms>(value, ignoreCase: true, out var result) ? result : null;

    private static DateTime? ParseDate(string? value) =>
        DateTime.TryParseExact(value, "yyyy-MM-dd HH:mm:ss",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var d) ? d : null;

    /// <summary>MILOS usa "groupage" in minuscolo per il valore GROUPAGE.</summary>
    private static string? MapCargoType(CargoType? cargoType) => cargoType switch
    {
        CargoType.FTL      => "FTL",
        CargoType.LTL      => "LTL",
        CargoType.GROUPAGE => "groupage",
        null               => null,
        _                  => cargoType.ToString()
    };
}

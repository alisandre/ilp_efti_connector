using ilp_efti_connector.Domain.Enums;
using ilp_efti_connector.Gateway.Contracts.Models;
using ilp_efti_connector.Gateway.EftiNative.Models.EN17532;

namespace ilp_efti_connector.Gateway.EftiNative.Mapping;

/// <summary>
/// Converte il modello interno <see cref="EcmrPayload"/> nel dataset EN 17532
/// per l'EFTI Gate nazionale (Fase 2), e viceversa.
/// </summary>
public static class EcmrPayloadToEftiMapper
{
    public static EftiEcmrDataset Map(EcmrPayload payload)
    {
        return new EftiEcmrDataset
        {
            Id            = payload.OperationCode,
            TypeCode      = payload.DatasetType,
            IssueDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),

            Consignor = new EftiConsignor
            {
                Name     = payload.Consignor.Name,
                TaxId    = payload.Consignor.VatNumber,
                EoriCode = payload.Consignor.EoriCode,
                Address  = MapAddress(payload.Consignor.Address)
            },

            Consignee = new EftiConsignee
            {
                Name       = payload.Consignee.Name,
                PlayerType = payload.Consignee.PlayerType.ToString(),
                TaxId      = payload.Consignee.TaxRegistration,
                EoriCode   = payload.Consignee.EoriCode,
                Address    = MapAddress(payload.Consignee.Address)
            },

            Carriers = payload.Carriers
                .OrderBy(c => c.SortOrder)
                .Select(MapCarrier)
                .ToList(),

            AcceptanceLocation = new EftiLocation
            {
                Address  = MapAddress(payload.PickupLocation.Address),
                DateTime = payload.PickupLocation.Date?.ToString("yyyy-MM-ddTHH:mm:ssZ")
            },

            DeliveryLocation = new EftiLocation
            {
                Address = MapAddress(payload.DeliveryLocation.Address)
            },

            ConsignmentItems = payload.Goods is not null
                ? MapGoods(payload.Goods)
                : null,

            TransportDetails = payload.TransportDetails.CargoType.HasValue || payload.TransportDetails.Incoterms.HasValue
                ? new EftiTransportDetails
                  {
                      CargoType = payload.TransportDetails.CargoType?.ToString(),
                      Incoterms = payload.TransportDetails.Incoterms?.ToString()
                  }
                : null,

            Hashcode = payload.Hashcode is not null
                ? new EftiHashcode { Value = payload.Hashcode.Value, Algorithm = payload.Hashcode.Algorithm }
                : null
        };
    }

    public static EcmrPayload MapBack(EftiEcmrDataset d)
    {
        return new EcmrPayload(
            OperationCode:  d.Id,
            DatasetType:    d.TypeCode,
            IsMasterCmr:    false,
            Note:           null,
            Consignor: new ConsignorInfo(
                Name:      d.Consignor.Name,
                VatNumber: d.Consignor.TaxId,
                EoriCode:  d.Consignor.EoriCode,
                Address:   MapAddressBack(d.Consignor.Address)),
            Consignee: new ConsigneeInfo(
                Name:            d.Consignee.Name,
                PlayerType:      ParsePlayerType(d.Consignee.PlayerType),
                TaxRegistration: d.Consignee.TaxId,
                EoriCode:        d.Consignee.EoriCode,
                Address:         MapAddressBack(d.Consignee.Address)),
            Carriers: d.Carriers
                .Select((c, i) => new CarrierInfo(
                    SortOrder:               i,
                    Name:                    c.Name,
                    PlayerType:              ParsePlayerType(c.PlayerType),
                    TaxRegistration:         c.TaxId,
                    EoriCode:                c.EoriCode,
                    TractorPlate:            c.TractorPlate ?? string.Empty,
                    TrailerPlate:            c.TrailerPlate,
                    TractorPlateCountryCode: c.TractorPlateCountryCode,
                    TrailerPlateCountryCode: c.TrailerPlateCountryCode,
                    EquipmentCategory:       ParseEquipmentCategory(c.EquipmentCategory),
                    Address:                 MapAddressBack(c.Address)))
                .ToList(),
            PickupLocation: new AcceptanceLocation(
                Address: MapAddressBack(d.AcceptanceLocation?.Address),
                Date:    ParseDateTime(d.AcceptanceLocation?.DateTime)),
            DeliveryLocation: new DeliveryLocation(
                Address: MapAddressBack(d.DeliveryLocation?.Address)),
            Goods: d.ConsignmentItems is not null ? MapGoodsBack(d.ConsignmentItems) : new ConsignmentItems(0, 0m, null, []),
            TransportDetails: new TransportDetailsInfo(
                CargoType: ParseCargoType(d.TransportDetails?.CargoType),
                Incoterms: ParseIncoterms(d.TransportDetails?.Incoterms)),
            Hashcode: d.Hashcode is not null
                ? new HashcodeInfo(d.Hashcode.Value, d.Hashcode.Algorithm)
                : null);
    }

    // ─── Forward helpers ─────────────────────────────────────────────────────

    private static EftiCarrier MapCarrier(CarrierInfo c) => new()
    {
        Name                    = c.Name,
        PlayerType              = c.PlayerType.ToString(),
        TaxId                   = c.TaxRegistration,
        EoriCode                = c.EoriCode,
        TractorPlate            = c.TractorPlate,
        TrailerPlate            = c.TrailerPlate,
        TractorPlateCountryCode = c.TractorPlateCountryCode,
        TrailerPlateCountryCode = c.TrailerPlateCountryCode,
        EquipmentCategory       = c.EquipmentCategory?.ToString(),
        Address                 = MapAddress(c.Address)
    };

    private static EftiAddress MapAddress(AddressInfo a) => new()
    {
        StreetName  = a.Street,
        PostCode    = a.PostalCode,
        CityName    = a.City,
        CountryCode = a.CountryCode,
        CountryName = a.CountryName
    };

    private static EftiGoods MapGoods(ConsignmentItems goods) => new()
    {
        TotalItemQuantity = goods.TotalItemQuantity,
        TotalWeight       = goods.TotalWeight,
        TotalVolume       = goods.TotalVolume,
        Packages = goods.Packages
            .Select(p => new EftiPackage
            {
                ShippingMarks = p.ShippingMarks,
                ItemQuantity  = p.ItemQuantity,
                TypeCode      = p.TypeCode,
                GrossWeight   = p.GrossWeight,
                GrossVolume   = p.GrossVolume
            })
            .ToList()
    };

    // ─── Reverse helpers ─────────────────────────────────────────────────────

    private static AddressInfo MapAddressBack(EftiAddress? a) =>
        a is not null
            ? new AddressInfo(a.StreetName ?? string.Empty, a.CityName, a.PostCode, a.CountryCode, a.CountryName, null)
            : new AddressInfo(string.Empty, string.Empty, null, string.Empty, null, null);

    private static ConsignmentItems MapGoodsBack(EftiGoods g) => new(
        TotalItemQuantity: g.TotalItemQuantity,
        TotalWeight:       g.TotalWeight,
        TotalVolume:       g.TotalVolume,
        Packages: g.Packages
            .Select((p, i) => new ConsignmentPackage(i, p.ShippingMarks, p.ItemQuantity, p.TypeCode, p.GrossWeight, p.GrossVolume))
            .ToList());

    private static PlayerType ParsePlayerType(string? value) =>
        Enum.TryParse<PlayerType>(value, ignoreCase: true, out var r) ? r : PlayerType.CARRIER;

    private static EcmrEquipmentCategory? ParseEquipmentCategory(string? value) =>
        Enum.TryParse<EcmrEquipmentCategory>(value, ignoreCase: true, out var r) ? r : null;

    private static CargoType? ParseCargoType(string? value) =>
        Enum.TryParse<CargoType>(value, ignoreCase: true, out var r) ? r : null;

    private static Incoterms? ParseIncoterms(string? value) =>
        Enum.TryParse<Incoterms>(value, ignoreCase: true, out var r) ? r : null;

    private static DateTime? ParseDateTime(string? value) =>
        DateTime.TryParse(value, out var dt) ? dt : null;
}

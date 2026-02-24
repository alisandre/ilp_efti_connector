using ilp_efti_connector.Application.Customers.Commands.UpsertCustomer;
using ilp_efti_connector.Application.TransportOperations.Commands.SubmitTransportOperation;
using ilp_efti_connector.Domain.Enums;
using ilp_efti_connector.Gateway.Contracts.Models;
using ilp_efti_connector.Shared.Contracts.Dtos;

namespace ilp_efti_connector.NormalizationService.Mapping;

/// <summary>
/// Converte <see cref="SourcePayloadDto"/> nei comandi e modelli necessari
/// alla pipeline di normalizzazione.
/// </summary>
public static class SourcePayloadMapper
{
    public static UpsertCustomerCommand ToUpsertCustomerCommand(SourcePayloadDto dto, Guid sourceId) =>
        new(
            CustomerCode:   dto.CustomerCode,
            BusinessName:   dto.CustomerName,
            VatNumber:      dto.CustomerVat,
            EoriCode:       dto.CustomerEori,
            SourceId:       sourceId,
            DestinationCode: dto.DestinationCode,
            AddressLine1:   dto.DeliveryLocation?.StreetName,
            City:           dto.DeliveryLocation?.CityName,
            PostalCode:     dto.DeliveryLocation?.PostCode,
            Province:       null,
            CountryCode:    dto.DeliveryLocation?.CountryCode,
            UnLocode:       null
        );

    public static SubmitTransportOperationCommand ToSubmitCommand(
        SourcePayloadDto dto,
        Guid sourceId,
        Guid customerId,
        Guid? destinationId,
        Guid transportOperationId,
        GatewayProvider gatewayProvider,
        string ecmrPayloadJson) =>
        new(
            SourceId:             sourceId,
            CustomerId:           customerId,
            DestinationId:        destinationId,
            OperationCode:        dto.OperationCode,
            DatasetType:          dto.DatasetType,
            TransportOperationId: transportOperationId,
            Consignee:            dto.Consignee is null ? null : MapConsignee(dto.Consignee),
            Carriers:             dto.Carriers.Select(MapCarrier).ToList(),
            Detail:               MapDetail(dto),
            ConsignmentItem:      dto.ConsignmentItems is null ? null : MapConsignment(dto.ConsignmentItems),
            RawPayloadJson:       ecmrPayloadJson,
            GatewayProvider:      gatewayProvider
        );

    public static EcmrPayload ToEcmrPayload(SourcePayloadDto dto, string customerName, string? vat, string? eori) =>
        new(
            OperationCode: dto.OperationCode,
            DatasetType:   dto.DatasetType,
            IsMasterCmr:   true,
            Note:          null,
            Consignor: new ConsignorInfo(
                Name:      customerName,
                VatNumber: vat,
                EoriCode:  eori,
                Address:   MapAddress(dto.ConsignorAddress)),
            Consignee: dto.Consignee is null
                ? new ConsigneeInfo(string.Empty, PlayerType.CARRIER, null, null, new AddressInfo(string.Empty, string.Empty, null, string.Empty, null, null))
                : new ConsigneeInfo(
                    Name:            dto.Consignee.Name,
                    PlayerType:      ParsePlayerType(dto.Consignee.PlayerType),
                    TaxRegistration: dto.Consignee.TaxRegistration,
                    EoriCode:        dto.Consignee.EoriCode,
                    Address:         new AddressInfo(
                        dto.Consignee.StreetName ?? string.Empty,
                        dto.Consignee.CityName,
                        dto.Consignee.PostCode,
                        dto.Consignee.CountryCode,
                        dto.Consignee.CountryName,
                        null)),
            Carriers: dto.Carriers.Select((c, i) => new CarrierInfo(
                SortOrder:               i,
                Name:                    c.Name,
                PlayerType:              ParsePlayerType(c.PlayerType),
                TaxRegistration:         c.TaxRegistration,
                EoriCode:                c.EoriCode,
                TractorPlate:            c.TractorPlate,
                TrailerPlate:            null,
                TractorPlateCountryCode: null,
                TrailerPlateCountryCode: null,
                EquipmentCategory:       ParseEquipmentCategory(c.EquipmentCategory),
                Address:                 new AddressInfo(c.StreetName ?? string.Empty, c.CityName, c.PostCode, c.CountryCode, c.CountryName, null))).ToList(),
            PickupLocation: new AcceptanceLocation(
                Address: MapAddress(dto.AcceptanceLocation is null ? null : new ConsignorAddressDto(
                    dto.AcceptanceLocation.StreetName,
                    dto.AcceptanceLocation.PostCode,
                    dto.AcceptanceLocation.CityName,
                    dto.AcceptanceLocation.CountryCode,
                    dto.AcceptanceLocation.CountryName)),
                Date: dto.AcceptanceLocation?.Date),
            DeliveryLocation: new DeliveryLocation(
                Address: MapAddress(dto.DeliveryLocation is null ? null : new ConsignorAddressDto(
                    dto.DeliveryLocation.StreetName,
                    dto.DeliveryLocation.PostCode,
                    dto.DeliveryLocation.CityName,
                    dto.DeliveryLocation.CountryCode,
                    dto.DeliveryLocation.CountryName))),
            Goods: dto.ConsignmentItems is null
                ? new ConsignmentItems(0, 0m, null, [])
                : new ConsignmentItems(
                    TotalItemQuantity: dto.ConsignmentItems.TotalItemQuantity,
                    TotalWeight:       dto.ConsignmentItems.TotalWeight,
                    TotalVolume:       dto.ConsignmentItems.TotalVolume,
                    Packages: dto.ConsignmentItems.Packages
                        .Select(p => new ConsignmentPackage(p.SortOrder, p.ShippingMarks, p.ItemQuantity, p.TypeCode, p.GrossWeight, p.GrossVolume))
                        .ToList()),
            TransportDetails: new TransportDetailsInfo(
                CargoType: ParseCargoType(dto.TransportDetails?.CargoType),
                Incoterms: ParseIncoterms(dto.TransportDetails?.Incoterms)),
            Hashcode: dto.Hashcode is null ? null : new HashcodeInfo(dto.Hashcode.Value, dto.Hashcode.Algorithm));

    // ─── Private helpers ─────────────────────────────────────────────────────

    private static AddressInfo MapAddress(ConsignorAddressDto? a) =>
        a is not null
            ? new AddressInfo(a.StreetName ?? string.Empty, a.CityName, a.PostCode, a.CountryCode, a.CountryName, null)
            : new AddressInfo(string.Empty, string.Empty, null, string.Empty, null, null);

    private static ConsigneeData MapConsignee(ConsigneeDto c) =>
        new(c.Name, ParsePlayerType(c.PlayerType), c.TaxRegistration, c.EoriCode,
            c.StreetName, c.PostCode, c.CityName, c.CountryCode, c.CountryName);

    private static CarrierData MapCarrier(CarrierDto c) =>
        new(c.SortOrder, c.Name, ParsePlayerType(c.PlayerType), c.TractorPlate,
            c.TaxRegistration, c.EoriCode, ParseEquipmentCategory(c.EquipmentCategory),
            c.StreetName, c.PostCode, c.CityName, c.CountryCode, c.CountryName);

    private static TransportDetailData? MapDetail(SourcePayloadDto dto)
    {
        var al = dto.AcceptanceLocation;
        var dl = dto.DeliveryLocation;
        var td = dto.TransportDetails;

        if (al is null && dl is null && td is null)
            return null;

        return new TransportDetailData(
            CargoType:             ParseCargoType(td?.CargoType),
            Incoterms:             ParseIncoterms(td?.Incoterms),
            AcceptanceStreetName:  al?.StreetName,
            AcceptancePostCode:    al?.PostCode,
            AcceptanceCityName:    al?.CityName,
            AcceptanceCountryCode: al?.CountryCode,
            AcceptanceCountryName: al?.CountryName,
            AcceptanceDate:        al?.Date,
            ReceiptStreetName:     dl?.StreetName,
            ReceiptPostCode:       dl?.PostCode,
            ReceiptCityName:       dl?.CityName,
            ReceiptCountryCode:    dl?.CountryCode,
            ReceiptCountryName:    dl?.CountryName);
    }

    private static ConsignmentData MapConsignment(ConsignmentItemsDto c) =>
        new(c.TotalItemQuantity, c.TotalWeight, c.TotalVolume,
            c.Packages.Select(p => new PackageData(p.SortOrder, p.ShippingMarks, p.ItemQuantity, p.TypeCode, p.GrossWeight, p.GrossVolume)).ToList());

    private static PlayerType ParsePlayerType(string? v) =>
        Enum.TryParse<PlayerType>(v, ignoreCase: true, out var r) ? r : PlayerType.CARRIER;

    private static EcmrEquipmentCategory? ParseEquipmentCategory(string? v) =>
        Enum.TryParse<EcmrEquipmentCategory>(v, ignoreCase: true, out var r) ? r : null;

    private static CargoType? ParseCargoType(string? v) =>
        Enum.TryParse<CargoType>(v, ignoreCase: true, out var r) ? r : null;

    private static Incoterms? ParseIncoterms(string? v) =>
        Enum.TryParse<Incoterms>(v, ignoreCase: true, out var r) ? r : null;
}

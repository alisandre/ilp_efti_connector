using ilp_efti_connector.Domain.Enums;
using ilp_efti_connector.Gateway.Contracts.Models;
using ilp_efti_connector.Gateway.Milos.Mapping;

namespace ilp_efti_connector.Gateway.Milos.Tests;

public sealed class EcmrPayloadToMilosMapperTests
{
    // ─── Map ────────────────────────────────────────────────────────────────

    [Fact]
    public void Map_ShouldPopulate_ShippingFields()
    {
        var result = EcmrPayloadToMilosMapper.Map(CreatePayload("CMR-001", "ECMR"));

        result.Shipping.ECMRId.Should().Be("CMR-001");
        result.Shipping.DatasetType.Should().Be("ECMR");
    }

    [Fact]
    public void Map_ShouldPopulate_ConsignorFields()
    {
        var result = EcmrPayloadToMilosMapper.Map(CreatePayload());

        result.ConsignorSender.Name.Should().Be("Mittente SRL");
        result.ConsignorSender.TaxRegistration.Should().Be("IT12345678901");
        result.ConsignorSender.EORICode.Should().Be("EORI001");
    }

    [Fact]
    public void Map_ShouldPreserve_CarrierCountAndPlate()
    {
        var result = EcmrPayloadToMilosMapper.Map(CreatePayload());

        result.Carriers.Should().ContainSingle();
        result.Carriers[0].Name.Should().Be("Vettore SRL");
        result.Carriers[0].TractorPlate.Should().Be("AB123CD");
    }

    [Fact]
    public void Map_ShouldMap_CargoType_Groupage_ToLowercase()
    {
        var result = EcmrPayloadToMilosMapper.Map(CreatePayload(cargoType: CargoType.GROUPAGE));

        result.TransportDetails?.CargoType.Should().Be("groupage");
    }

    [Fact]
    public void Map_ShouldMap_CargoType_FTL_ToUppercase()
    {
        var result = EcmrPayloadToMilosMapper.Map(CreatePayload(cargoType: CargoType.FTL));

        result.TransportDetails?.CargoType.Should().Be("FTL");
    }

    [Fact]
    public void Map_WhenNoTransportDetails_ShouldLeave_TransportDetailsNull()
    {
        var result = EcmrPayloadToMilosMapper.Map(CreatePayload(cargoType: null, incoterms: null));

        result.TransportDetails.Should().BeNull();
    }

    // ─── MapBack (round-trip) ────────────────────────────────────────────────

    [Fact]
    public void MapBack_ShouldRoundTrip_OperationCode()
    {
        var payload  = CreatePayload("CMR-ROUNDTRIP");
        var request  = EcmrPayloadToMilosMapper.Map(payload);
        var restored = EcmrPayloadToMilosMapper.MapBack(request);

        restored.OperationCode.Should().Be(payload.OperationCode);
    }

    [Fact]
    public void MapBack_ShouldRoundTrip_ConsignorName()
    {
        var payload  = CreatePayload();
        var request  = EcmrPayloadToMilosMapper.Map(payload);
        var restored = EcmrPayloadToMilosMapper.MapBack(request);

        restored.Consignor.Name.Should().Be(payload.Consignor.Name);
    }

    [Fact]
    public void MapBack_ShouldRoundTrip_CarrierCount()
    {
        var payload  = CreatePayload();
        var request  = EcmrPayloadToMilosMapper.Map(payload);
        var restored = EcmrPayloadToMilosMapper.MapBack(request);

        restored.Carriers.Should().HaveCount(payload.Carriers.Count);
    }

    [Fact]
    public void MapBack_ShouldRoundTrip_GoodsWeight()
    {
        var payload  = CreatePayload();
        var request  = EcmrPayloadToMilosMapper.Map(payload);
        var restored = EcmrPayloadToMilosMapper.MapBack(request);

        restored.Goods.TotalWeight.Should().Be(payload.Goods.TotalWeight);
    }

    // ─── Factory ────────────────────────────────────────────────────────────

    private static EcmrPayload CreatePayload(
        string     code       = "TEST-001",
        string     datasetType = "ECMR",
        CargoType? cargoType  = CargoType.FTL,
        Incoterms? incoterms  = Incoterms.DAP) => new(
        OperationCode:    code,
        DatasetType:      datasetType,
        IsMasterCmr:      false,
        Note:             null,
        Consignor: new ConsignorInfo("Mittente SRL", "IT12345678901", "EORI001",
            new AddressInfo("Via Roma 1", "Milano", "20100", "IT", "Italy", null)),
        Consignee: new ConsigneeInfo("Destinatario SRL", PlayerType.CONSIGNEE, "IT98765432100", null,
            new AddressInfo("Via Torino 5", "Roma", "00100", "IT", "Italy", null)),
        Carriers:
        [
            new CarrierInfo(0, "Vettore SRL", PlayerType.CARRIER, "IT11111111111", null,
                "AB123CD", "XY789EF", "IT", "IT", null,
                new AddressInfo("Via Milano 10", "Torino", "10100", "IT", "Italy", null))
        ],
        PickupLocation:   new AcceptanceLocation(
            new AddressInfo("Via Pickup 1", "Bologna", "40100", "IT", "Italy", null),
            new DateTime(2024, 1, 15, 9, 0, 0)),
        DeliveryLocation: new DeliveryLocation(
            new AddressInfo("Via Delivery 5", "Napoli", "80100", "IT", "Italy", null)),
        Goods: new ConsignmentItems(10, 500.5m, 2.3m,
            [new ConsignmentPackage(0, "MARK001", 10, "PLT", 500.5m, 2.3m)]),
        TransportDetails: new TransportDetailsInfo(cargoType, incoterms),
        Hashcode:         null);
}

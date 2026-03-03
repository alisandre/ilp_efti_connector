using ilp_efti_connector.Domain.Enums;
using ilp_efti_connector.Gateway.Contracts.Models;
using ilp_efti_connector.Gateway.EftiNative.Mapping;

namespace ilp_efti_connector.Gateway.EftiNative.Tests;

public sealed class EcmrPayloadToEftiMapperTests
{
    // ─── Map (Forward) ────────────────────────────────────────────────────────

    [Fact]
    public void Map_ShouldPopulate_IdAndTypeCode()
    {
        var result = EcmrPayloadToEftiMapper.Map(BuildPayload("CMR-001", "ECMR"));

        result.Id.Should().Be("CMR-001");
        result.TypeCode.Should().Be("ECMR");
    }

    [Fact]
    public void Map_ShouldPopulate_IssueDateTime_AsIso8601()
    {
        var result = EcmrPayloadToEftiMapper.Map(BuildPayload("CMR-002"));

        result.IssueDateTime.Should().MatchRegex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z$");
    }

    [Fact]
    public void Map_ShouldPopulate_ConsignorFields()
    {
        var result = EcmrPayloadToEftiMapper.Map(BuildPayload());

        result.Consignor.Name.Should().Be("Mittente SRL");
        result.Consignor.TaxId.Should().Be("IT12345678901");
        result.Consignor.EoriCode.Should().Be("EORI001");
    }

    [Fact]
    public void Map_ShouldPopulate_ConsigneeFields()
    {
        var result = EcmrPayloadToEftiMapper.Map(BuildPayload());

        result.Consignee.Name.Should().Be("Destinatario SRL");
        result.Consignee.PlayerType.Should().Be("CONSIGNEE");
    }

    [Fact]
    public void Map_ShouldPreserve_CarrierOrderAndPlate()
    {
        var result = EcmrPayloadToEftiMapper.Map(BuildPayload());

        result.Carriers.Should().ContainSingle();
        result.Carriers[0].Name.Should().Be("Vettore SRL");
        result.Carriers[0].TractorPlate.Should().Be("AB123CD");
    }

    [Fact]
    public void Map_ShouldSet_AcceptanceLocation_DateTime()
    {
        var result = EcmrPayloadToEftiMapper.Map(BuildPayload());

        result.AcceptanceLocation.Should().NotBeNull();
        result.AcceptanceLocation!.DateTime.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Map_ShouldLeave_Hashcode_Null_WhenNotProvided()
    {
        var result = EcmrPayloadToEftiMapper.Map(BuildPayload());

        result.Hashcode.Should().BeNull();
    }

    [Fact]
    public void Map_ShouldPopulate_Hashcode_WhenProvided()
    {
        var payload  = BuildPayload();
        var modified = payload with { Hashcode = new HashcodeInfo("abc123", "SHA-256") };

        var result = EcmrPayloadToEftiMapper.Map(modified);

        result.Hashcode.Should().NotBeNull();
        result.Hashcode!.Value.Should().Be("abc123");
        result.Hashcode.Algorithm.Should().Be("SHA-256");
    }

    // ─── MapBack (Round-trip) ─────────────────────────────────────────────────

    [Fact]
    public void MapBack_ShouldRoundTrip_OperationCode()
    {
        var payload  = BuildPayload("RT-001");
        var dataset  = EcmrPayloadToEftiMapper.Map(payload);
        var restored = EcmrPayloadToEftiMapper.MapBack(dataset);

        restored.OperationCode.Should().Be(payload.OperationCode);
    }

    [Fact]
    public void MapBack_ShouldRoundTrip_ConsignorName()
    {
        var payload  = BuildPayload();
        var dataset  = EcmrPayloadToEftiMapper.Map(payload);
        var restored = EcmrPayloadToEftiMapper.MapBack(dataset);

        restored.Consignor.Name.Should().Be(payload.Consignor.Name);
    }

    [Fact]
    public void MapBack_ShouldRoundTrip_CarrierCount()
    {
        var payload  = BuildPayload();
        var dataset  = EcmrPayloadToEftiMapper.Map(payload);
        var restored = EcmrPayloadToEftiMapper.MapBack(dataset);

        restored.Carriers.Should().HaveCount(payload.Carriers.Count);
    }

    [Fact]
    public void MapBack_ShouldRoundTrip_TractorPlate()
    {
        var payload  = BuildPayload();
        var dataset  = EcmrPayloadToEftiMapper.Map(payload);
        var restored = EcmrPayloadToEftiMapper.MapBack(dataset);

        restored.Carriers[0].TractorPlate.Should().Be(payload.Carriers[0].TractorPlate);
    }

    // ─── Factory ─────────────────────────────────────────────────────────────

    private static EcmrPayload BuildPayload(string code = "TEST-001", string datasetType = "ECMR") => new(
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
                "AB123CD", null, "IT", null, null,
                new AddressInfo("Via Milano 10", "Torino", "10100", "IT", "Italy", null))
        ],
        PickupLocation:   new AcceptanceLocation(
            new AddressInfo("Via Pickup 1", "Bologna", "40100", "IT", "Italy", null),
            new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc)),
        DeliveryLocation: new DeliveryLocation(
            new AddressInfo("Via Delivery 5", "Napoli", "80100", "IT", "Italy", null)),
        Goods:            new ConsignmentItems(10, 500m, null, []),
        TransportDetails: new TransportDetailsInfo(null, null),
        Hashcode:         null);
}

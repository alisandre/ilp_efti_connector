using ilp_efti_connector.Domain.Enums;
using ilp_efti_connector.Gateway.Contracts.Exceptions;
using ilp_efti_connector.Gateway.Contracts.Models;
using ilp_efti_connector.Gateway.Milos.Client;
using ilp_efti_connector.Gateway.Milos.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Refit;
using System.Net;

namespace ilp_efti_connector.Gateway.Milos.Tests;

public sealed class MilosTfpGatewayTests
{
    private readonly Mock<IMilosEcmrClient> _client = new();
    private readonly MilosTfpGateway        _sut;

    public MilosTfpGatewayTests()
        => _sut = new MilosTfpGateway(_client.Object, NullLogger<MilosTfpGateway>.Instance);

    // ─── SendEcmrAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task SendEcmrAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var response = MockResponse<ECMRResponse>(true,
            new ECMRResponse { ECMRId = "CMR-001", Uuid = "uuid-abc" });
        _client.Setup(c => c.CreateEcmrAsync(It.IsAny<ECMRRequest>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response.Object);

        var result = await _sut.SendEcmrAsync(CreatePayload());

        result.IsSuccess.Should().BeTrue();
        result.ExternalId.Should().Be("CMR-001");
        result.ExternalUuid.Should().Be("uuid-abc");
    }

    [Fact]
    public async Task SendEcmrAsync_WhenServerError_ReturnsFailureResult()
    {
        var response = MockResponse<ECMRResponse>(false,
            statusCode: HttpStatusCode.InternalServerError, reasonPhrase: "Internal Server Error");
        _client.Setup(c => c.CreateEcmrAsync(It.IsAny<ECMRRequest>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response.Object);

        var result = await _sut.SendEcmrAsync(CreatePayload());

        result.IsSuccess.Should().BeFalse();
        result.HttpStatusCode.Should().Be(500);
    }

    [Fact]
    public async Task SendEcmrAsync_WhenUnauthorized_ThrowsGatewayAuthenticationException()
    {
        var response = MockResponse<ECMRResponse>(false, statusCode: HttpStatusCode.Unauthorized);
        _client.Setup(c => c.CreateEcmrAsync(It.IsAny<ECMRRequest>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response.Object);

        await _sut.Invoking(s => s.SendEcmrAsync(CreatePayload()))
                  .Should().ThrowAsync<GatewayAuthenticationException>();
    }

    [Fact]
    public async Task SendEcmrAsync_WhenForbidden_ThrowsGatewayAuthenticationException()
    {
        var response = MockResponse<ECMRResponse>(false, statusCode: HttpStatusCode.Forbidden);
        _client.Setup(c => c.CreateEcmrAsync(It.IsAny<ECMRRequest>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response.Object);

        await _sut.Invoking(s => s.SendEcmrAsync(CreatePayload()))
                  .Should().ThrowAsync<GatewayAuthenticationException>();
    }

    // ─── UpdateEcmrAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateEcmrAsync_WhenSuccess_ReturnsSuccessWithExternalId()
    {
        var response = MockVoidResponse(true, HttpStatusCode.OK);
        _client.Setup(c => c.UpdateEcmrAsync("CMR-001", It.IsAny<ECMRRequest>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response.Object);

        var result = await _sut.UpdateEcmrAsync("CMR-001", CreatePayload());

        result.IsSuccess.Should().BeTrue();
        result.ExternalId.Should().Be("CMR-001");
    }

    [Fact]
    public async Task UpdateEcmrAsync_WhenServerError_ReturnsFailureResult()
    {
        var response = MockVoidResponse(false, HttpStatusCode.BadGateway);
        _client.Setup(c => c.UpdateEcmrAsync(It.IsAny<string>(), It.IsAny<ECMRRequest>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response.Object);

        var result = await _sut.UpdateEcmrAsync("CMR-001", CreatePayload());

        result.IsSuccess.Should().BeFalse();
        result.HttpStatusCode.Should().Be(502);
    }

    // ─── DeleteEcmrAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteEcmrAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var response = MockVoidResponse(true, HttpStatusCode.NoContent);
        _client.Setup(c => c.DeleteEcmrAsync("CMR-001", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response.Object);

        var result = await _sut.DeleteEcmrAsync("CMR-001");

        result.IsSuccess.Should().BeTrue();
        result.ExternalId.Should().Be("CMR-001");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static Mock<IApiResponse<T>> MockResponse<T>(
        bool success,
        T? content = default,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        string? reasonPhrase = null)
    {
        var mock = new Mock<IApiResponse<T>>();
        mock.Setup(r => r.IsSuccessStatusCode).Returns(success);
        mock.Setup(r => r.StatusCode).Returns(statusCode);
        mock.Setup(r => r.ReasonPhrase).Returns(reasonPhrase);
        if (content is not null) mock.Setup(r => r.Content).Returns(content);
        return mock;
    }

    private static Mock<IApiResponse> MockVoidResponse(
        bool success,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        string? reasonPhrase = null)
    {
        var mock = new Mock<IApiResponse>();
        mock.Setup(r => r.IsSuccessStatusCode).Returns(success);
        mock.Setup(r => r.StatusCode).Returns(statusCode);
        mock.Setup(r => r.ReasonPhrase).Returns(reasonPhrase);
        return mock;
    }

    private static EcmrPayload CreatePayload(string code = "TEST-001") => new(
        OperationCode:    code,
        DatasetType:      "ECMR",
        IsMasterCmr:      false,
        Note:             null,
        Consignor: new ConsignorInfo("Mittente SRL", "IT12345678901", null,
            new AddressInfo("Via Roma 1", "Milano", "20100", "IT", null, null)),
        Consignee: new ConsigneeInfo("Destinatario SRL", PlayerType.CONSIGNEE, null, null,
            new AddressInfo("Via Torino 5", "Roma", "00100", "IT", null, null)),
        Carriers:
        [
            new CarrierInfo(0, "Vettore SRL", PlayerType.CARRIER, null, null,
                "AB123CD", null, "IT", null, null,
                new AddressInfo("Via Milano 10", "Torino", "10100", "IT", null, null))
        ],
        PickupLocation:   new AcceptanceLocation(
            new AddressInfo("Via Pickup 1", "Bologna", "40100", "IT", null, null),
            DateTime.UtcNow),
        DeliveryLocation: new DeliveryLocation(
            new AddressInfo("Via Delivery 5", "Napoli", "80100", "IT", null, null)),
        Goods:            new ConsignmentItems(1, 100m, null, []),
        TransportDetails: new TransportDetailsInfo(null, null),
        Hashcode:         null);
}

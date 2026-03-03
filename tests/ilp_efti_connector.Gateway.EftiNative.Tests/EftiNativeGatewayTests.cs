using ilp_efti_connector.Domain.Enums;
using ilp_efti_connector.Gateway.Contracts.Exceptions;
using ilp_efti_connector.Gateway.Contracts.Models;
using ilp_efti_connector.Gateway.EftiNative.Client;
using ilp_efti_connector.Gateway.EftiNative.Models.EN17532;
using Microsoft.Extensions.Logging.Abstractions;
using Refit;
using System.Net;

namespace ilp_efti_connector.Gateway.EftiNative.Tests;

public sealed class EftiNativeGatewayTests
{
    private readonly Mock<IEftiGateClient> _client = new();
    private readonly EftiNativeGateway     _sut;

    public EftiNativeGatewayTests()
        => _sut = new EftiNativeGateway(_client.Object, NullLogger<EftiNativeGateway>.Instance);

    // ─── SendEcmrAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task SendEcmrAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var response = MockResponse<EftiSubmitResponse>(
            true, new EftiSubmitResponse { MessageId = "MSG-001" });
        _client.Setup(c => c.CreateDatasetAsync(It.IsAny<EftiEcmrDataset>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response.Object);

        var result = await _sut.SendEcmrAsync(CreatePayload());

        result.IsSuccess.Should().BeTrue();
        result.ExternalId.Should().Be("MSG-001");
    }

    [Fact]
    public async Task SendEcmrAsync_WhenServerError_ReturnsFailureResult()
    {
        var response = MockResponse<EftiSubmitResponse>(
            false, statusCode: HttpStatusCode.InternalServerError);
        _client.Setup(c => c.CreateDatasetAsync(It.IsAny<EftiEcmrDataset>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response.Object);

        var result = await _sut.SendEcmrAsync(CreatePayload());

        result.IsSuccess.Should().BeFalse();
        result.HttpStatusCode.Should().Be(500);
    }

    [Fact]
    public async Task SendEcmrAsync_WhenUnauthorized_ReturnsFailureWithAuthError()
    {
        var response = MockResponse<EftiSubmitResponse>(
            false, statusCode: HttpStatusCode.Unauthorized);
        _client.Setup(c => c.CreateDatasetAsync(It.IsAny<EftiEcmrDataset>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response.Object);

        var result = await _sut.SendEcmrAsync(CreatePayload());

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("AUTH_ERROR");
    }

    [Fact]
    public async Task SendEcmrAsync_WhenUnprocessableEntity_ReturnsValidationError()
    {
        var response = MockResponse<EftiSubmitResponse>(
            false, statusCode: HttpStatusCode.UnprocessableEntity);
        _client.Setup(c => c.CreateDatasetAsync(It.IsAny<EftiEcmrDataset>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response.Object);

        var result = await _sut.SendEcmrAsync(CreatePayload());

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }

    // ─── UpdateEcmrAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateEcmrAsync_WhenSuccess_ReturnsSuccessWithExternalId()
    {
        var response = MockVoidResponse(true, HttpStatusCode.OK);
        _client.Setup(c => c.UpdateDatasetAsync("MSG-001", It.IsAny<EftiEcmrDataset>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response.Object);

        var result = await _sut.UpdateEcmrAsync("MSG-001", CreatePayload());

        result.IsSuccess.Should().BeTrue();
        result.ExternalId.Should().Be("MSG-001");
    }

    [Fact]
    public async Task UpdateEcmrAsync_WhenServerError_ReturnsFailureResult()
    {
        var response = MockVoidResponse(false, HttpStatusCode.BadGateway);
        _client.Setup(c => c.UpdateDatasetAsync(It.IsAny<string>(), It.IsAny<EftiEcmrDataset>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response.Object);

        var result = await _sut.UpdateEcmrAsync("MSG-001", CreatePayload());

        result.IsSuccess.Should().BeFalse();
        result.HttpStatusCode.Should().Be(502);
    }

    // ─── DeleteEcmrAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteEcmrAsync_WhenSuccess_ReturnsSuccessResult()
    {
        var response = MockVoidResponse(true, HttpStatusCode.NoContent);
        _client.Setup(c => c.DeleteDatasetAsync("MSG-001", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response.Object);

        var result = await _sut.DeleteEcmrAsync("MSG-001");

        result.IsSuccess.Should().BeTrue();
        result.ExternalId.Should().Be("MSG-001");
    }

    // ─── GetEcmrAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetEcmrAsync_WhenSuccess_ReturnsMappedPayload()
    {
        var dataset = new EftiEcmrDataset
        {
            Id       = "MSG-001",
            TypeCode = "ECMR",
            Consignor = new() { Name = "Mittente SRL" },
            Consignee = new() { Name = "Destinatario SRL", PlayerType = "CONSIGNEE" },
            Carriers  = [new() { Name = "Vettore SRL", TractorPlate = "AB123CD", PlayerType = "CARRIER" }],
            AcceptanceLocation = new() { Address = new() },
            DeliveryLocation   = new() { Address = new() }
        };
        var response = MockResponse<EftiEcmrDataset>(true, dataset);
        _client.Setup(c => c.GetDatasetAsync("MSG-001", It.IsAny<CancellationToken>()))
               .ReturnsAsync(response.Object);

        var payload = await _sut.GetEcmrAsync("MSG-001");

        payload.OperationCode.Should().Be("MSG-001");
        payload.Consignor.Name.Should().Be("Mittente SRL");
    }

    [Fact]
    public async Task GetEcmrAsync_WhenNotFound_ThrowsGatewayException()
    {
        var response = MockResponse<EftiEcmrDataset>(false, statusCode: HttpStatusCode.NotFound);
        _client.Setup(c => c.GetDatasetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(response.Object);

        await _sut.Invoking(s => s.GetEcmrAsync("NOT-FOUND"))
                  .Should().ThrowAsync<GatewayException>();
    }

    // ─── HealthCheckAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task HealthCheckAsync_WhenHealthy_ReturnsHealthyStatus()
    {
        var response = MockVoidResponse(true, HttpStatusCode.OK);
        _client.Setup(c => c.HealthCheckAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(response.Object);

        var status = await _sut.HealthCheckAsync();

        status.IsHealthy.Should().BeTrue();
        status.Provider.Should().Be("EFTI_NATIVE");
    }

    [Fact]
    public async Task HealthCheckAsync_WhenUnreachable_ReturnsUnhealthyStatus()
    {
        _client.Setup(c => c.HealthCheckAsync(It.IsAny<CancellationToken>()))
               .ThrowsAsync(new HttpRequestException("Connection refused"));

        var status = await _sut.HealthCheckAsync();

        status.IsHealthy.Should().BeFalse();
        status.ErrorMessage.Should().Contain("Connection refused");
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

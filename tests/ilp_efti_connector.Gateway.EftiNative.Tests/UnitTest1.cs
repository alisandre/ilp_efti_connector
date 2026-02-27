using ilp_efti_connector.Gateway.EftiNative.Client;
using ilp_efti_connector.Gateway.EftiNative.Models.EN17532;
using Moq;
using ilp_efti_connector.Gateway.Contracts.Models;
using Refit;

namespace ilp_efti_connector.Gateway.EftiNative.Tests;

public class UnitTest1
{
    [Fact]
    public async Task SendEcmrAsync_ReturnsError_WhenResponseIsNotSuccess()
    {
        var clientMock = new Moq.Mock<IEftiGateClient>();
        var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<EftiNativeGateway>>();

        var payload = new EcmrPayload(
            "OP123",
            "ECMR",
            false,
            null,
            new ConsignorInfo(
                "Consignor",
                string.Empty,
                string.Empty,              
                new AddressInfo("Street", "City", "12345", "IT", null, null)
            ),
            new ConsigneeInfo(
                "Consignee",
                ilp_efti_connector.Domain.Enums.PlayerType.CONSIGNEE,
                string.Empty,
                string.Empty,
                new AddressInfo("Street", "City", "12345", "IT", null, null)
            ),
            new System.Collections.Generic.List<CarrierInfo> {
                new CarrierInfo(
                    1,
                    "Carrier",
                    ilp_efti_connector.Domain.Enums.PlayerType.CARRIER,
                    default,
                    default,
                    default,
                    default,
                    default,
                    default,
                    default,
                    new AddressInfo("Street", "City", "12345", "IT", null, null)
                )
            },
            new AcceptanceLocation(
                new AddressInfo("Street", "City", "12345", "IT", null, null),
                null
            ),
            new DeliveryLocation(
                new AddressInfo("Street", "City", "12345", "IT", null, null)
            ),
            new ConsignmentItems(
                1,
                10.0m,
                null,
                new System.Collections.Generic.List<ConsignmentPackage> {
                    new ConsignmentPackage(1, null, 1, null, 10.0m, null)
                }
            ),
            new TransportDetailsInfo(null, null),
            null
        );

        // Crea una risposta fittizia del tipo corretto (Refit.ApiResponse)
        var errorHttpResponse = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
        {
            ReasonPhrase = "BadRequest"
        };
        var settings = new RefitSettings
        {
            ContentSerializer = new Refit.SystemTextJsonContentSerializer()
        };
        var errorApiResponse = new Refit.ApiResponse<EftiSubmitResponse>(errorHttpResponse, null, settings);
        clientMock.Setup(c => c.CreateDatasetAsync(Moq.It.IsAny<EftiEcmrDataset>(), Moq.It.IsAny<CancellationToken>())).ReturnsAsync(errorApiResponse);

        var gateway = new EftiNativeGateway(clientMock.Object, loggerMock.Object);
        var result = await gateway.SendEcmrAsync(payload);

        Assert.False(result.IsSuccess);
        Assert.Equal("VALIDATION_ERROR", result.ErrorCode);
        Assert.Equal("Payload non valido: Invalid data", result.ErrorMessage);
        Assert.Equal((int)System.Net.HttpStatusCode.BadRequest, result.HttpStatusCode);
    }

    [Fact]
    public async Task SendEcmrAsync_ReturnsSuccess_WhenResponseIsValid()
    {
        var clientMock = new Moq.Mock<IEftiGateClient>();
        var loggerMock = new Moq.Mock<Microsoft.Extensions.Logging.ILogger<EftiNativeGateway>>();

        var payload = new EcmrPayload(
            "OP456",
            "ECMR",
            false,
            null,
            new ConsignorInfo(
                "Consignor",
                null,
                null,
                new AddressInfo("Street", "City", "12345", "IT", null, null)
            ),
            new ConsigneeInfo(
                "Consignee",
                ilp_efti_connector.Domain.Enums.PlayerType.CONSIGNEE,
                null,
                null,
                new AddressInfo("Street", "City", "12345", "IT", null, null)
            ),
            new List<CarrierInfo> {
                new CarrierInfo(
                    1,
                    "Carrier",
                    ilp_efti_connector.Domain.Enums.PlayerType.CARRIER,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    null,
                    new AddressInfo("Street", "City", "12345", "IT", null, null)
                )
            },
            new AcceptanceLocation(
                new AddressInfo("Street", "City", "12345", "IT", null, null),
                null
            ),
            new DeliveryLocation(
                new AddressInfo("Street", "City", "12345", "IT", null, null)
            ),
            new ConsignmentItems(
                1,
                10.0m,
                null,
                new System.Collections.Generic.List<ConsignmentPackage> {
                    new ConsignmentPackage(1, null, 1, null, 10.0m, null)
                }
            ),
            new TransportDetailsInfo(null, null),
            null
        );

        var submitResponse = new EftiSubmitResponse { MessageId = "MSG789", Status = "ACCEPTED" };
        var successHttpResponse = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK);
        var settings = new RefitSettings
        {
            ContentSerializer = new Refit.SystemTextJsonContentSerializer()
        };
        var successApiResponse = new Refit.ApiResponse<EftiSubmitResponse>(successHttpResponse, submitResponse, settings);
        clientMock.Setup(c => c.CreateDatasetAsync(Moq.It.IsAny<EftiEcmrDataset>(), Moq.It.IsAny<CancellationToken>())).ReturnsAsync(successApiResponse);

        var gateway = new EftiNativeGateway(clientMock.Object, loggerMock.Object);
        var result = await gateway.SendEcmrAsync(payload);

        Assert.True(result.IsSuccess);
        Assert.Equal("MSG789", result.ExternalId);
        Assert.Equal((int)System.Net.HttpStatusCode.OK, result.HttpStatusCode);
        Assert.Null(result.ErrorMessage);
    }
}

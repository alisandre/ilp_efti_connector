using ilp_efti_connector.Gateway.EftiNative.Models.EN17532;
using Refit;

namespace ilp_efti_connector.Gateway.EftiNative.Client;

/// <summary>
/// Client Refit per le API REST dell'EFTI Gate nazionale (Fase 2).
/// L'autenticazione OAuth2 è delegata a <see cref="EftiOAuth2Handler"/>.
/// </summary>
public interface IEftiGateClient
{
    [Post("/datasets")]
    Task<ApiResponse<EftiSubmitResponse>> CreateDatasetAsync([Body] EftiEcmrDataset dataset, CancellationToken ct = default);

    [Put("/datasets/{id}")]
    Task<IApiResponse> UpdateDatasetAsync(string id, [Body] EftiEcmrDataset dataset, CancellationToken ct = default);

    [Delete("/datasets/{id}")]
    Task<IApiResponse> DeleteDatasetAsync(string id, CancellationToken ct = default);

    [Get("/datasets/{id}")]
    Task<ApiResponse<EftiEcmrDataset>> GetDatasetAsync(string id, CancellationToken ct = default);

    [Get("/health")]
    Task<IApiResponse> HealthCheckAsync(CancellationToken ct = default);
}

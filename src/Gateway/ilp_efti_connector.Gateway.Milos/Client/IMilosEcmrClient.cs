using ilp_efti_connector.Gateway.Milos.Models;
using Refit;

namespace ilp_efti_connector.Gateway.Milos.Client;

/// <summary>Client Refit per le API MILOS e-CMR Service (Circle SpA ICD v1.0).</summary>
public interface IMilosEcmrClient
{
    [Post("/ecmr")]
    Task<IApiResponse<ECMRResponse>> CreateEcmrAsync([Body] ECMRRequest request, CancellationToken ct = default);

    [Put("/ecmr/{id}")]
    Task<IApiResponse> UpdateEcmrAsync([AliasAs("id")] string id, [Body] ECMRRequest request, CancellationToken ct = default);

    [Delete("/ecmr/{id}")]
    Task<IApiResponse> DeleteEcmrAsync([AliasAs("id")] string id, CancellationToken ct = default);

    [Get("/ecmr/get/{id}")]
    Task<IApiResponse<ECMRRequest>> GetEcmrAsync([AliasAs("id")] string id, CancellationToken ct = default);
}

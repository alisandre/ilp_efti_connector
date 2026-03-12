using ilp_efti_connector.Gateway.EftiNative.Models.EN17532;
using Refit;

namespace ilp_efti_connector.Gateway.EftiNative.Client;

/// <summary>
/// Client Refit per le API REST dell'EFTI Gate nazionale (Fase 2).
/// L'autenticazione OAuth2 è delegata a <see cref="EftiOAuth2Handler"/>.
/// Il mutual TLS è configurato nell'HttpClientHandler in <c>EftiNativeGatewayExtensions</c>.
/// </summary>
public interface IEftiGateClient
{
    /// <summary>Invia un nuovo dataset eFTI al Gate. Risponde 202 Accepted (asincrono).</summary>
    [Post("/datasets")]
    Task<IApiResponse<EftiSubmitResponse>> CreateDatasetAsync([Body] EftiEcmrDataset dataset, CancellationToken ct = default);

    /// <summary>Aggiorna un dataset esistente.</summary>
    [Put("/datasets/{id}")]
    Task<IApiResponse> UpdateDatasetAsync(string id, [Body] EftiEcmrDataset dataset, CancellationToken ct = default);

    /// <summary>Cancella un dataset (richiede dataset in stato non-finale).</summary>
    [Delete("/datasets/{id}")]
    Task<IApiResponse> DeleteDatasetAsync(string id, CancellationToken ct = default);

    /// <summary>Recupera il dataset completo per ID.</summary>
    [Get("/datasets/{id}")]
    Task<IApiResponse<EftiEcmrDataset>> GetDatasetAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Polling dello stato del dataset. Usato come fallback quando il callback
    /// asincrono non arriva entro il timeout atteso.
    /// </summary>
    [Get("/datasets/{id}/status")]
    Task<IApiResponse<EftiDatasetStatus>> GetDatasetStatusAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Registra (o aggiorna) il callback URL per le notifiche di stato.
    /// Chiamato al boot dell'<c>EftiGatewayService</c> via <c>GatewayHealthMonitor</c>.
    /// </summary>
    [Post("/notifications/register")]
    Task<IApiResponse> RegisterCallbackAsync([Body] EftiCallbackRegistration registration, CancellationToken ct = default);

    /// <summary>Health check del Gate.</summary>
    [Get("/health")]
    Task<IApiResponse> HealthCheckAsync(CancellationToken ct = default);
}

/// <summary>Payload per la registrazione del callback URL sul Gate.</summary>
public sealed record EftiCallbackRegistration(
    [property: System.Text.Json.Serialization.JsonPropertyName("callbackUrl")]   string CallbackUrl,
    [property: System.Text.Json.Serialization.JsonPropertyName("platformId")]    string PlatformId,
    [property: System.Text.Json.Serialization.JsonPropertyName("countryCode")]   string CountryCode
);


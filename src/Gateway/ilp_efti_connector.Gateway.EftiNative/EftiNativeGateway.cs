using ilp_efti_connector.Gateway.Contracts;
using ilp_efti_connector.Gateway.Contracts.Exceptions;
using ilp_efti_connector.Gateway.Contracts.Models;
using ilp_efti_connector.Gateway.EftiNative.Client;
using ilp_efti_connector.Gateway.EftiNative.Mapping;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;

namespace ilp_efti_connector.Gateway.EftiNative;

/// <summary>
/// Implementazione di <see cref="IEftiGateway"/> per la Fase 2 — integrazione diretta
/// con l'EFTI Gate nazionale. Converte <see cref="EcmrPayload"/> in dataset EN 17532.
/// L'autenticazione OAuth2 è gestita da <see cref="Client.EftiOAuth2Handler"/>.
/// </summary>
public sealed class EftiNativeGateway : IEftiGateway
{
    private const string ProviderName = "EFTI_NATIVE";

    private readonly IEftiGateClient _client;
    private readonly ILogger<EftiNativeGateway> _logger;

    public EftiNativeGateway(IEftiGateClient client, ILogger<EftiNativeGateway> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<EftiSendResult> SendEcmrAsync(EcmrPayload payload, CancellationToken ct = default)
    {
        var dataset = EcmrPayloadToEftiMapper.Map(payload);
        _logger.LogDebug("EFTI_NATIVE SendEcmr → Id={Id}", payload.OperationCode);

        var response = await _client.CreateDatasetAsync(dataset, ct);

        if (!response.IsSuccessStatusCode || response.Content is null)
        {
            var error = response.Error?.Content ?? response.ReasonPhrase ?? "Unknown error";
            _logger.LogWarning("EFTI_NATIVE SendEcmr KO: {StatusCode} — {Error}", response.StatusCode, error);
            return HandleErrorResponse(response.StatusCode, error);
        }

        _logger.LogInformation("EFTI_NATIVE SendEcmr OK: messageId={MessageId}", response.Content.MessageId);
        return EftiSendResult.Success(response.Content.MessageId, httpStatusCode: (int)response.StatusCode);
    }

    public async Task<EftiSendResult> UpdateEcmrAsync(string externalId, EcmrPayload payload, CancellationToken ct = default)
    {
        var dataset = EcmrPayloadToEftiMapper.Map(payload);
        _logger.LogDebug("EFTI_NATIVE UpdateEcmr → Id={Id}", externalId);

        var response = await _client.UpdateDatasetAsync(externalId, dataset, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = response.Error?.Content ?? response.ReasonPhrase ?? "Unknown error";
            _logger.LogWarning("EFTI_NATIVE UpdateEcmr KO: {StatusCode} — {Error}", response.StatusCode, error);
            return HandleErrorResponse(response.StatusCode, error);
        }

        _logger.LogInformation("EFTI_NATIVE UpdateEcmr OK: Id={Id}", externalId);
        return EftiSendResult.Success(externalId, httpStatusCode: (int)response.StatusCode);
    }

    public async Task<EftiSendResult> DeleteEcmrAsync(string externalId, CancellationToken ct = default)
    {
        _logger.LogDebug("EFTI_NATIVE DeleteEcmr → Id={Id}", externalId);

        var response = await _client.DeleteDatasetAsync(externalId, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = response.Error?.Content ?? response.ReasonPhrase ?? "Unknown error";
            _logger.LogWarning("EFTI_NATIVE DeleteEcmr KO: {StatusCode} — {Error}", response.StatusCode, error);
            return HandleErrorResponse(response.StatusCode, error);
        }

        _logger.LogInformation("EFTI_NATIVE DeleteEcmr OK: Id={Id}", externalId);
        return EftiSendResult.Success(externalId, httpStatusCode: (int)response.StatusCode);
    }

    public async Task<EcmrPayload> GetEcmrAsync(string externalId, CancellationToken ct = default)
    {
        var response = await _client.GetDatasetAsync(externalId, ct);

        if (!response.IsSuccessStatusCode || response.Content is null)
        {
            var error = response.Error?.Content ?? response.ReasonPhrase ?? "Unknown error";
            throw new GatewayException(ProviderName, $"GetEcmr fallito per id={externalId}: {error}", (int)response.StatusCode);
        }

        return EcmrPayloadToEftiMapper.MapBack(response.Content);
    }

    public async Task<GatewayHealthStatus> HealthCheckAsync(CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var response = await _client.HealthCheckAsync(ct);
            sw.Stop();

            return response.IsSuccessStatusCode
                ? GatewayHealthStatus.Healthy(ProviderName, sw.Elapsed)
                : GatewayHealthStatus.Unhealthy(ProviderName, $"HTTP {(int)response.StatusCode}", sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogWarning(ex, "EFTI_NATIVE HealthCheck non raggiungibile.");
            return GatewayHealthStatus.Unhealthy(ProviderName, ex.Message, sw.Elapsed);
        }
    }

    private static EftiSendResult HandleErrorResponse(HttpStatusCode statusCode, string error)
    {
        var code = (int)statusCode;
        return statusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden
                => EftiSendResult.Failure($"Autenticazione EFTI_NATIVE fallita: {error}", "AUTH_ERROR", code),
            HttpStatusCode.TooManyRequests
                => EftiSendResult.Failure($"Rate limit EFTI_NATIVE superato: {error}", "RATE_LIMIT", code),
            HttpStatusCode.UnprocessableEntity or HttpStatusCode.BadRequest
                => EftiSendResult.Failure($"Payload non valido: {error}", "VALIDATION_ERROR", code),
            _ => EftiSendResult.Failure($"Errore EFTI_NATIVE [{code}]: {error}", "GATEWAY_ERROR", code)
        };
    }
}

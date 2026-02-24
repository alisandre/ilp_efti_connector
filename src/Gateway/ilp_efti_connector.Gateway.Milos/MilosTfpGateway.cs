using ilp_efti_connector.Gateway.Contracts;
using ilp_efti_connector.Gateway.Contracts.Exceptions;
using ilp_efti_connector.Gateway.Contracts.Models;
using ilp_efti_connector.Gateway.Milos.Client;
using ilp_efti_connector.Gateway.Milos.Hashing;
using ilp_efti_connector.Gateway.Milos.Mapping;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;

namespace ilp_efti_connector.Gateway.Milos;

/// <summary>
/// Implementazione di <see cref="IEftiGateway"/> per la Fase 1 — integrazione via MILOS TFP (Circle SpA).
/// Converte il modello interno <see cref="EcmrPayload"/> in ECMRRequest MILOS,
/// calcola l'hash SHA-256 e chiama le API REST <c>/api/ecmr-service/</c>.
/// </summary>
public sealed class MilosTfpGateway : IEftiGateway
{
    private const string ProviderName = "MILOS";

    private readonly IMilosEcmrClient _client;
    private readonly ILogger<MilosTfpGateway> _logger;

    public MilosTfpGateway(IMilosEcmrClient client, ILogger<MilosTfpGateway> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<EftiSendResult> SendEcmrAsync(EcmrPayload payload, CancellationToken ct = default)
    {
        var request = EcmrPayloadToMilosMapper.Map(payload);
        request.HashcodeDetails = MilosHashcodeCalculator.Compute(request);

        _logger.LogDebug("MILOS SendEcmr → eCMRID={Id}", payload.OperationCode);

        var response = await _client.CreateEcmrAsync(request, ct);

        if (!response.IsSuccessStatusCode || response.Content is null)
        {
            var error = response.Error?.Content ?? response.ReasonPhrase ?? "Unknown error";
            _logger.LogWarning("MILOS SendEcmr KO: {StatusCode} — {Error}", response.StatusCode, error);
            return HandleErrorResponse(response.StatusCode, error);
        }

        _logger.LogInformation("MILOS SendEcmr OK: eCMRID={Id} uuid={Uuid}",
            response.Content.ECMRId, response.Content.Uuid);

        return EftiSendResult.Success(response.Content.ECMRId, response.Content.Uuid, (int)response.StatusCode);
    }

    public async Task<EftiSendResult> UpdateEcmrAsync(string externalId, EcmrPayload payload, CancellationToken ct = default)
    {
        var request = EcmrPayloadToMilosMapper.Map(payload);
        request.HashcodeDetails = MilosHashcodeCalculator.Compute(request);

        _logger.LogDebug("MILOS UpdateEcmr → eCMRID={Id}", externalId);

        var response = await _client.UpdateEcmrAsync(externalId, request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = response.Error?.Content ?? response.ReasonPhrase ?? "Unknown error";
            _logger.LogWarning("MILOS UpdateEcmr KO: {StatusCode} — {Error}", response.StatusCode, error);
            return HandleErrorResponse(response.StatusCode, error);
        }

        _logger.LogInformation("MILOS UpdateEcmr OK: eCMRID={Id}", externalId);
        return EftiSendResult.Success(externalId, httpStatusCode: (int)response.StatusCode);
    }

    public async Task<EftiSendResult> DeleteEcmrAsync(string externalId, CancellationToken ct = default)
    {
        _logger.LogDebug("MILOS DeleteEcmr → eCMRID={Id}", externalId);

        var response = await _client.DeleteEcmrAsync(externalId, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = response.Error?.Content ?? response.ReasonPhrase ?? "Unknown error";
            _logger.LogWarning("MILOS DeleteEcmr KO: {StatusCode} — {Error}", response.StatusCode, error);
            return HandleErrorResponse(response.StatusCode, error);
        }

        _logger.LogInformation("MILOS DeleteEcmr OK: eCMRID={Id}", externalId);
        return EftiSendResult.Success(externalId, httpStatusCode: (int)response.StatusCode);
    }

    public async Task<EcmrPayload> GetEcmrAsync(string externalId, CancellationToken ct = default)
    {
        var response = await _client.GetEcmrAsync(externalId, ct);

        if (!response.IsSuccessStatusCode || response.Content is null)
        {
            var error = response.Error?.Content ?? response.ReasonPhrase ?? "Unknown error";
            throw new GatewayException(ProviderName, $"GetEcmr fallito per eCMRID={externalId}: {error}", (int)response.StatusCode);
        }

        return EcmrPayloadToMilosMapper.MapBack(response.Content);
    }

    public async Task<GatewayHealthStatus> HealthCheckAsync(CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            // Usa una GET su un id inesistente: se MILOS risponde (anche 404) è raggiungibile
            var response = await _client.GetEcmrAsync("__healthcheck__", ct);
            sw.Stop();

            var isReachable = response.StatusCode != HttpStatusCode.ServiceUnavailable
                           && response.StatusCode != HttpStatusCode.GatewayTimeout;

            return isReachable
                ? GatewayHealthStatus.Healthy(ProviderName, sw.Elapsed)
                : GatewayHealthStatus.Unhealthy(ProviderName, $"HTTP {(int)response.StatusCode}", sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return GatewayHealthStatus.Unhealthy(ProviderName, ex.Message, sw.Elapsed);
        }
    }

    private static EftiSendResult HandleErrorResponse(HttpStatusCode statusCode, string errorMessage)
    {
        if (statusCode == HttpStatusCode.Unauthorized || statusCode == HttpStatusCode.Forbidden)
            throw new GatewayAuthenticationException(ProviderName, $"Autenticazione MILOS fallita: {errorMessage}");

        return EftiSendResult.Failure(errorMessage, statusCode.ToString(), (int)statusCode);
    }
}

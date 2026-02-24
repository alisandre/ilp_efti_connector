using ilp_efti_connector.Gateway.Contracts.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ilp_efti_connector.Gateway.EftiNative.Client;

/// <summary>
/// DelegatingHandler che gestisce il token OAuth2 (client_credentials) verso l'EFTI Gate.
/// Il token è cached in Redis con TTL = expires_in − 60s.
/// Se CertificatePath è configurato, aggiunge il certificato X.509 per mutual TLS.
/// </summary>
public sealed class EftiOAuth2Handler : DelegatingHandler
{
    private const string ProviderName = "EFTI_NATIVE";

    private readonly EftiNativeOptions _options;
    private readonly Auth.EftiTokenCache _tokenCache;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EftiOAuth2Handler> _logger;

    public EftiOAuth2Handler(
        IOptions<EftiNativeOptions> options,
        Auth.EftiTokenCache tokenCache,
        IHttpClientFactory httpClientFactory,
        ILogger<EftiOAuth2Handler> logger)
    {
        _options          = options.Value;
        _tokenCache       = tokenCache;
        _httpClientFactory = httpClientFactory;
        _logger           = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var token = await GetOrRefreshTokenAsync(ct);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, ct);
    }

    private async Task<string> GetOrRefreshTokenAsync(CancellationToken ct)
    {
        var cached = await _tokenCache.GetAsync(_options.ClientId, ct);
        if (cached is not null)
            return cached;

        _logger.LogDebug("EFTI_NATIVE: token cache miss — acquisisco nuovo token.");
        var response = await FetchTokenAsync(ct);

        var ttl = TimeSpan.FromSeconds(Math.Max(response.ExpiresIn - 60, 30));
        await _tokenCache.SetAsync(_options.ClientId, response.AccessToken, ttl, ct);

        _logger.LogInformation("EFTI_NATIVE: token OAuth2 acquisito, scade in {ExpiresIn}s.", response.ExpiresIn);
        return response.AccessToken;
    }

    private async Task<Auth.EftiTokenResponse> FetchTokenAsync(CancellationToken ct)
    {
        using var httpClient = _httpClientFactory.CreateClient("EftiTokenClient");

        var formFields = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "client_credentials"),
            new("client_id",  _options.ClientId),
            new("scope",      _options.Scope)
        };

        if (_options.ClientSecret is not null)
            formFields.Add(new("client_secret", _options.ClientSecret));

        var httpResponse = await httpClient.PostAsync(
            _options.TokenEndpoint,
            new FormUrlEncodedContent(formFields),
            ct);

        if (!httpResponse.IsSuccessStatusCode)
        {
            var body = await httpResponse.Content.ReadAsStringAsync(ct);
            throw new GatewayAuthenticationException(
                ProviderName,
                $"Token fetch fallito [{(int)httpResponse.StatusCode}]: {body}");
        }

        var json = await httpResponse.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<Auth.EftiTokenResponse>(json)
            ?? throw new GatewayAuthenticationException(ProviderName, "Risposta token non valida.");
    }
}

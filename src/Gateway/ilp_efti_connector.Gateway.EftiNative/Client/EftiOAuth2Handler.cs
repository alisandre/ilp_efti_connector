using ilp_efti_connector.Gateway.Contracts.Exceptions;
using ilp_efti_connector.Gateway.EftiNative.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using EftiCertLoader = ilp_efti_connector.Gateway.EftiNative.Auth.X509CertificateLoader;

namespace ilp_efti_connector.Gateway.EftiNative.Client;

/// <summary>
/// DelegatingHandler che gestisce il token OAuth2 (client_credentials) verso l'EFTI Gate.
/// <list type="bullet">
///   <item>Il token è cached in Redis con TTL = expires_in − 60 s per evitare race condition.</item>
///   <item>
///     Se <see cref="EftiNativeOptions.UseClientAssertion"/> = true e
///     <see cref="EftiNativeOptions.CertificatePath"/> è configurato, usa
///     <c>client_assertion</c> JWT firmato con chiave privata RSA del certificato X.509
///     (RFC 7523 — obbligatorio per produzione eIDAS).
///     Altrimenti usa <c>client_secret</c> (solo sviluppo/test).
///   </item>
/// </list>
/// </summary>
public sealed class EftiOAuth2Handler : DelegatingHandler
{
    private const string ProviderName = "EFTI_NATIVE";

    private readonly EftiNativeOptions _options;
    private readonly EftiTokenCache _tokenCache;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EftiOAuth2Handler> _logger;

    public EftiOAuth2Handler(
        IOptions<EftiNativeOptions> options,
        EftiTokenCache tokenCache,
        IHttpClientFactory httpClientFactory,
        ILogger<EftiOAuth2Handler> logger)
    {
        _options           = options.Value;
        _tokenCache        = tokenCache;
        _httpClientFactory = httpClientFactory;
        _logger            = logger;
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

        _logger.LogInformation("EFTI_NATIVE: token OAuth2 acquisito ({Mode}), scade in {ExpiresIn}s.",
            _options.UseClientAssertion ? "client_assertion/JWT" : "client_secret",
            response.ExpiresIn);

        return response.AccessToken;
    }

    private async Task<EftiTokenResponse> FetchTokenAsync(CancellationToken ct)
    {
        using var httpClient = _httpClientFactory.CreateClient("EftiTokenClient");

        var formFields = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "client_credentials"),
            new("client_id",  _options.ClientId),
            new("scope",      _options.Scope)
        };

        if (_options.UseClientAssertion && !string.IsNullOrEmpty(_options.CertificatePath))
        {
            // RFC 7523 — JWT Bearer client authentication con X.509
            var cert = EftiCertLoader.Load(_options.CertificatePath, _options.CertificatePassword);
            var jwt  = BuildClientAssertionJwt(cert, _options.ClientId, _options.TokenEndpoint);

            formFields.Add(new("client_assertion_type",
                "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"));
            formFields.Add(new("client_assertion", jwt));

            _logger.LogDebug("EFTI_NATIVE: uso client_assertion JWT (X.509 thumbprint={Thumb}).",
                Convert.ToHexString(cert.GetCertHash()).ToLowerInvariant()[..8]);
        }
        else if (_options.ClientSecret is not null)
        {
            formFields.Add(new("client_secret", _options.ClientSecret));
        }
        else
        {
            _logger.LogWarning("EFTI_NATIVE: né client_secret né client_assertion configurati. " +
                               "La richiesta token potrebbe fallire.");
        }

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
        return JsonSerializer.Deserialize<EftiTokenResponse>(json)
            ?? throw new GatewayAuthenticationException(ProviderName, "Risposta token non valida.");
    }

    // ─── RFC 7523 — JWT costruito con BCL puro (nessuna dipendenza esterna) ──

    /// <summary>
    /// Costruisce un JWT firmato RS256 con la chiave privata del certificato X.509,
    /// conforme a RFC 7523 §2.2 per client authentication verso OAuth2.
    /// </summary>
    private static string BuildClientAssertionJwt(
        X509Certificate2 cert, string clientId, string tokenEndpoint)
    {
        static string B64Url(byte[] bytes) =>
            Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        // Header: alg=RS256 + x5t (SHA-1 thumbprint del certificato)
        var header = B64Url(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
        {
            alg = "RS256",
            typ = "JWT",
            x5t = B64Url(cert.GetCertHash())
        })));

        // Payload: claims standard RFC 7523
        var now = DateTimeOffset.UtcNow;
        var body = B64Url(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
        {
            iss = clientId,
            sub = clientId,
            aud = tokenEndpoint,
            jti = Guid.NewGuid().ToString("N"),
            iat = now.ToUnixTimeSeconds(),
            exp = now.AddMinutes(5).ToUnixTimeSeconds()
        })));

        // Firma RS256 con la chiave privata RSA del certificato
        var signingInput = $"{header}.{body}";
        using var rsa = cert.GetRSAPrivateKey()
            ?? throw new InvalidOperationException(
                "Il certificato X.509 non contiene una chiave privata RSA. " +
                "Verificare che il file .pfx includa la chiave privata.");

        var signature = rsa.SignData(
            Encoding.UTF8.GetBytes(signingInput),
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        return $"{signingInput}.{B64Url(signature)}";
    }
}

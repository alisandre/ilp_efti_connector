using Microsoft.Extensions.Options;

namespace ilp_efti_connector.Gateway.Milos.Client;

/// <summary>
/// Inietta l'header <c>X-API-Key</c> in ogni richiesta HTTP verso MILOS.
/// </summary>
public sealed class MilosApiKeyHandler : DelegatingHandler
{
    private const string ApiKeyHeader = "X-API-Key";
    private readonly MilosGatewayOptions _options;

    public MilosApiKeyHandler(IOptions<MilosGatewayOptions> options)
        => _options = options.Value;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.TryAddWithoutValidation(ApiKeyHeader, _options.ApiKey);
        return base.SendAsync(request, cancellationToken);
    }
}

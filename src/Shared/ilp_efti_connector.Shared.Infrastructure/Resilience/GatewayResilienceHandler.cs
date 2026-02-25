using Polly;

namespace ilp_efti_connector.Shared.Infrastructure.Resilience;

/// <summary>
/// DelegatingHandler che applica la pipeline Polly v8 (Retry → CircuitBreaker → Timeout)
/// a ogni richiesta HTTP in uscita verso un gateway esterno.
/// </summary>
public sealed class GatewayResilienceHandler : DelegatingHandler
{
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

    public GatewayResilienceHandler(ResiliencePipeline<HttpResponseMessage> pipeline)
        => _pipeline = pipeline;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
        => await _pipeline.ExecuteAsync(
            async ct => await base.SendAsync(request, ct),
            cancellationToken);
}

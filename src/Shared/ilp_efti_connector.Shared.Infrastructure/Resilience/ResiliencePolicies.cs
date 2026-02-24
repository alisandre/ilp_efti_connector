using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace ilp_efti_connector.Shared.Infrastructure.Resilience;

/// <summary>
/// Factory per pipeline Polly v8 da usare nelle chiamate HTTP verso gateway esterni
/// (MILOS TFP o EFTI Gate).
/// </summary>
public static class ResiliencePolicies
{
    /// <summary>
    /// Pipeline combinata: Retry esponenziale → Circuit Breaker → Timeout.
    /// Configurazione di default: 3 retry, CB dopo 5 errori su 30 s, timeout 30 s.
    /// </summary>
    public static ResiliencePipeline<HttpResponseMessage> CreateGatewayPipeline(
        int retryCount          = 3,
        int circuitBreakCount   = 5,
        int circuitBreakSeconds = 30,
        int timeoutSeconds      = 30)
    {
        return new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = retryCount,
                BackoffType      = DelayBackoffType.Exponential,
                Delay            = TimeSpan.FromSeconds(1),
                UseJitter        = true,
                ShouldHandle     = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(r => (int)r.StatusCode >= 500)
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                FailureRatio      = 0.5,
                SamplingDuration  = TimeSpan.FromSeconds(circuitBreakSeconds),
                MinimumThroughput = circuitBreakCount,
                BreakDuration     = TimeSpan.FromSeconds(circuitBreakSeconds),
                ShouldHandle      = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(r => (int)r.StatusCode >= 500)
            })
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(timeoutSeconds)
            })
            .Build();
    }

    /// <summary>Pipeline leggera per chiamate interne con solo retry + timeout.</summary>
    public static ResiliencePipeline<HttpResponseMessage> CreateLightPipeline(
        int retryCount     = 2,
        int timeoutSeconds = 10)
    {
        return new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = retryCount,
                BackoffType      = DelayBackoffType.Constant,
                Delay            = TimeSpan.FromMilliseconds(500),
                ShouldHandle     = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(r => (int)r.StatusCode >= 500)
            })
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(timeoutSeconds)
            })
            .Build();
    }
}

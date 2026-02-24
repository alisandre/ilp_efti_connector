using ilp_efti_connector.Shared.Infrastructure.Metrics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ilp_efti_connector.Shared.Infrastructure.Extensions;

public static class TelemetryExtensions
{
    public static IServiceCollection AddIlpEftiTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        var serviceVersion = configuration["App:Version"] ?? "1.0.0";

        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(serviceName, serviceVersion: serviceVersion))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddMeter(IlpEftiMetrics.MeterName)
                .AddPrometheusExporter());

        return services;
    }
}

using ilp_efti_connector.Gateway.Contracts;
using ilp_efti_connector.Gateway.EftiNative.Auth;
using ilp_efti_connector.Gateway.EftiNative.Client;
using ilp_efti_connector.Shared.Infrastructure.Extensions;
using ilp_efti_connector.Shared.Infrastructure.Resilience;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ilp_efti_connector.Gateway.EftiNative.DependencyInjection;

public static class EftiNativeGatewayExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy   = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Registra il gateway EFTI Nativo (Fase 2): Refit client, OAuth2 handler, token cache Redis.
    /// Richiede la sezione <c>EftiGateway:EftiNative</c> in appsettings.json.
    /// </summary>
    public static IServiceCollection AddEftiNativeGateway(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<EftiNativeOptions>(configuration.GetSection("EftiGateway:EftiNative"));

        // Redis per token cache
        services.AddIlpEftiRedis(configuration);
        services.AddSingleton<EftiTokenCache>();
        services.AddTransient<EftiOAuth2Handler>();

        // HTTP client senza auth handler — usato da EftiOAuth2Handler per il token fetch
        services.AddHttpClient("EftiTokenClient", (sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<EftiNativeOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
        });

        // Refit client principale verso EFTI Gate (con OAuth2 handler)
        services
            .AddRefitClient<IEftiGateClient>(new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer(_jsonOptions)
            })
            .ConfigureHttpClient((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<EftiNativeOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl);
                client.Timeout     = TimeSpan.FromSeconds(opts.TimeoutSeconds);
            })
            .AddHttpMessageHandler(_ => new GatewayResilienceHandler(ResiliencePolicies.CreateGatewayPipeline()))
            .AddHttpMessageHandler<EftiOAuth2Handler>();

        services.AddScoped<IEftiGateway, EftiNativeGateway>();

        return services;
    }
}

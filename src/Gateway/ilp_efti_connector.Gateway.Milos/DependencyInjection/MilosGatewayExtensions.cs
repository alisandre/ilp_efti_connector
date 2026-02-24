using ilp_efti_connector.Gateway.Contracts;
using ilp_efti_connector.Gateway.Milos.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ilp_efti_connector.Gateway.Milos.DependencyInjection;

public static class MilosGatewayExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy   = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static IServiceCollection AddMilosGateway(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MilosGatewayOptions>(configuration.GetSection("EftiGateway:Milos"));
        services.AddTransient<MilosApiKeyHandler>();

        services
            .AddRefitClient<IMilosEcmrClient>(new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer(_jsonOptions)
            })
            .ConfigureHttpClient((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<MilosGatewayOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl);
                client.Timeout     = TimeSpan.FromSeconds(opts.TimeoutSeconds);
            })
            .AddHttpMessageHandler<MilosApiKeyHandler>();

        services.AddScoped<IEftiGateway, MilosTfpGateway>();

        return services;
    }
}

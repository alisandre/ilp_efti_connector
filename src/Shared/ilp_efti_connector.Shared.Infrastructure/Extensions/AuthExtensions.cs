using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ilp_efti_connector.Shared.Infrastructure.Extensions;

public static class AuthExtensions
{
    /// <summary>
    /// Registra autenticazione JWT Bearer con Keycloak come authority.
    /// Configurazione attesa in <c>appsettings.json</c>:
    /// <code>
    /// "Keycloak": {
    ///   "Authority": "https://keycloak.example.com/realms/efti",
    ///   "Audience":  "efti-connector",
    ///   "RequireHttpsMetadata": "true"
    /// }
    /// </code>
    /// </summary>
    public static IServiceCollection AddIlpEftiAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority            = configuration["Keycloak:Authority"];
                options.Audience             = configuration["Keycloak:Audience"];
                options.RequireHttpsMetadata = bool.Parse(
                    configuration["Keycloak:RequireHttpsMetadata"] ?? "true");
            });

        services.AddAuthorization();

        return services;
    }
}

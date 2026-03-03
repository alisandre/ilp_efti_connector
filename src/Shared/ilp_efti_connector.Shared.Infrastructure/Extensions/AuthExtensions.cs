using ilp_efti_connector.Application.Common.Interfaces;
using ilp_efti_connector.Shared.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ilp_efti_connector.Shared.Infrastructure.Extensions;

public static class AuthExtensions
{
    /// <summary>
    /// Registra autenticazione JWT Bearer con Keycloak come authority.
    /// Configurazione attesa in <c>appsettings.json</c>:
    /// <code>
    /// "Keycloak": {
    ///   "Authority": "https://keycloak.example.com/realms/efti",
    ///   "Audience":  "efti-api",
    ///   "RequireHttpsMetadata": "true",
    ///   "ValidateAudience": "true"
    /// }
    /// </code>
    /// Impostare <c>ValidateAudience: false</c> in Development finché il token
    /// Keycloak non include il claim <c>aud</c> tramite audience mapper.
    /// </summary>
    public static IServiceCollection AddIlpEftiAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var validateAudience = bool.Parse(
            configuration["Keycloak:ValidateAudience"] ?? "true");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority            = configuration["Keycloak:Authority"];
                options.Audience             = configuration["Keycloak:Audience"];
                options.RequireHttpsMetadata = bool.Parse(
                    configuration["Keycloak:RequireHttpsMetadata"] ?? "true");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = validateAudience,
                };
            });

        services.AddAuthorization();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();

        return services;
    }
}

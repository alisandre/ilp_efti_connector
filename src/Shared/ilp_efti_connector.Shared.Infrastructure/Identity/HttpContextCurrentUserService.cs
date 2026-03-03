using ilp_efti_connector.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ilp_efti_connector.Shared.Infrastructure.Identity;

/// <summary>
/// Implementazione di <see cref="ICurrentUserService"/> per i servizi web
/// (ASP.NET Core). Estrae l'identità dell'utente autenticato dal JWT Keycloak
/// tramite <see cref="IHttpContextAccessor"/>.
/// </summary>
public sealed class HttpContextCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var sub = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User?.FindFirstValue("sub");
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? Username
        => User?.FindFirstValue(ClaimTypes.Name)
        ?? User?.FindFirstValue("preferred_username");

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}

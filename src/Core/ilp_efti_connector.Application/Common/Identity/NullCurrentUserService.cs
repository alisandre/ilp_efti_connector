using ilp_efti_connector.Application.Common.Interfaces;

namespace ilp_efti_connector.Application.Common.Identity;

/// <summary>
/// Implementazione no-op di <see cref="ICurrentUserService"/> per i Worker Service
/// (nessun contesto HTTP). L'audit verrà registrato senza <c>PerformedByUserId</c>.
/// </summary>
public sealed class NullCurrentUserService : ICurrentUserService
{
    public Guid?   UserId          => null;
    public string? Username        => null;
    public bool    IsAuthenticated => false;
}

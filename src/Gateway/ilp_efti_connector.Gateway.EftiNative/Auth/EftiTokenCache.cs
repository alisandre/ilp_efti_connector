using Microsoft.Extensions.Caching.Distributed;

namespace ilp_efti_connector.Gateway.EftiNative.Auth;

/// <summary>
/// Cache Redis per i token OAuth2 verso l'EFTI Gate.
/// TTL = <c>expires_in - 60s</c> per evitare race condition sulla scadenza.
/// </summary>
public sealed class EftiTokenCache
{
    private readonly IDistributedCache _cache;

    public EftiTokenCache(IDistributedCache cache) => _cache = cache;

    public Task<string?> GetAsync(string clientId, CancellationToken ct = default)
        => _cache.GetStringAsync(CacheKey(clientId), ct);

    public Task SetAsync(string clientId, string token, TimeSpan expiry, CancellationToken ct = default)
        => _cache.SetStringAsync(
            CacheKey(clientId),
            token,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiry },
            ct);

    public Task RemoveAsync(string clientId, CancellationToken ct = default)
        => _cache.RemoveAsync(CacheKey(clientId), ct);

    private static string CacheKey(string clientId) => $"efti_native_token_{clientId}";
}

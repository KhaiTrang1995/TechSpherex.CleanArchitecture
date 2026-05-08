namespace TechSpherex.CleanArchitecture.Application.Abstractions.Caching;

/// <summary>
/// Clean cache abstraction for the Application layer.
/// Wraps HybridCache (L1 In-Memory + L2 Redis) so handlers 
/// never depend on infrastructure packages directly.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a value from cache or creates it using the factory.
    /// L1 (RAM) → L2 (Redis) → Factory fallback.
    /// </summary>
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        TimeSpan? localExpiration = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>Sets a value directly in both L1 and L2 cache.</summary>
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        TimeSpan? localExpiration = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>Removes a specific key from all cache layers.</summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Invalidates all entries tagged with the given tag.</summary>
    Task InvalidateByTagAsync(string tag, CancellationToken cancellationToken = default);
}

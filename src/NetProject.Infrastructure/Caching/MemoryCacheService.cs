using Microsoft.Extensions.Caching.Memory;
using NetProject.Application.Abstractions.Caching;

namespace NetProject.Infrastructure.Caching;

public sealed class MemoryCacheService(IMemoryCache cache) : ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        => Task.FromResult(cache.TryGetValue(key, out var value) ? (T?)value : default);

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        cache.Set(key, value, ttl);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        cache.Remove(key);
        return Task.CompletedTask;
    }
}


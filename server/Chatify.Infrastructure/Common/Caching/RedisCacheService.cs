using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Infrastructure.Common.Caching.Extensions;
using StackExchange.Redis;

namespace Chatify.Infrastructure.Common.Caching;

internal sealed class RedisCacheService(IDatabase cache) : ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        => cache.GetAsync<T>(key);

    public Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = default, CancellationToken cancellationToken = default)
        => cache.SetAsync(key, value, expiry);
}
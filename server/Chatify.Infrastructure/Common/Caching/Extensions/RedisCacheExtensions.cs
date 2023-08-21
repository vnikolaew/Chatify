using Chatify.Shared.Abstractions.Serialization;
using Chatify.Shared.Infrastructure.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Chatify.Infrastructure.Common.Caching.Extensions;

public static class RedisCacheExtensions
{
    private static readonly ISerializer Serializer = new SystemTextJsonSerializer();

    public static async Task<T?> GetAsync<T>(this IDatabase database, string key)
    {
        var value = await database.StringGetAsync(new RedisKey(key));
        return Serializer.Deserialize<T>(value.ToString());
    }

    public static async Task<T?> GetAsync<T>(this IDistributedCache cache, string key)
    {
        var value = await cache.GetStringAsync(key);
        return Serializer.Deserialize<T>(value);
    }

    public static async Task<bool> SetAsync<T>(
        this IDistributedCache cache,
        string key,
        T value,
        TimeSpan? expiry = default)
    {
        await cache.SetStringAsync(key, Serializer.Serialize(value),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry
            });
        return true;
    }

    public static async Task<IEnumerable<T?>> GetAsync<T>(this IDatabase database, IEnumerable<string> keys)
    {
        var values = await database.StringGetAsync(keys.Select(_ => new RedisKey(_)).ToArray());
        return values
            .Select(v => v.HasValue
                ? Serializer.Deserialize<T>(v.ToString())
                : default);
    }

    public static Task<bool> SetAsync<T>(
        this IDatabase database,
        string key,
        T value,
        TimeSpan? expiry = default)
        => database.StringSetAsync(new RedisKey(key), new RedisValue(Serializer.Serialize(value)), expiry);

    public static Task<RedisResult> BloomFilterAddAsync<T>(
        this IDatabase database,
        string key,
        T value)
        => database.ExecuteAsync("BF.ADD", key, new RedisValue(Serializer.Serialize(value)));

    public static async Task<bool> BloomFilterExistsAsync(
        this IDatabase database,
        string filterKey,
        string itemKey)
        => ( bool )await database.ExecuteAsync("BF.EXISTS", filterKey, itemKey);

    public static async Task<IEnumerable<T?>> SetMembersAsync<T>(
        this IDatabase database, string key)
        => ( await database.SetMembersAsync(key) )
            .Select(v => Serializer.Deserialize<T>(v.ToString()));

    public static Task<bool> SetAsync<T>(this IDatabase database, IEnumerable<KeyValuePair<string, T>> values)
        => database.StringSetAsync(
            values.Select(kv => new KeyValuePair<RedisKey, RedisValue>(
                    new RedisKey(kv.Key),
                    new RedisValue(Serializer.Serialize(kv.Value))))
                .ToArray());
}
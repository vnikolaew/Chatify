using Chatify.Shared.Abstractions.Serialization;
using Chatify.Shared.Infrastructure.Serialization;
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

    public static async Task<IEnumerable<T?>> GetAsync<T>(this IDatabase database, IEnumerable<string> keys)
    {
        var values = await database.StringGetAsync(keys.Select(_ => new RedisKey(_)).ToArray());
        return values.Select(v => Serializer.Deserialize<T>(v.ToString()));
    }

    public static Task<bool> SetAsync<T>(this IDatabase database, string key, T value)
        => database.StringSetAsync(new RedisKey(key), new RedisValue(Serializer.Serialize(value)));

    public static Task<bool> SetAsync<T>(this IDatabase database, KeyValuePair<string, T>[] values)
        => database.StringSetAsync(
            values.Select(kv => new KeyValuePair<RedisKey, RedisValue>(
                    new RedisKey(kv.Key),
                    new RedisValue(Serializer.Serialize(kv.Value))))
                .ToArray());
}
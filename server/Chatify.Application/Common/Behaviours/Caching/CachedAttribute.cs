namespace Chatify.Application.Common.Behaviours.Caching;

[AttributeUsage(AttributeTargets.Class)]
public class CachedAttribute(string queryCacheKeyPrefix,
    uint ttlInSeconds = 0) : Attribute
{
    public string QueryCacheKeyPrefix { get; set; } = queryCacheKeyPrefix;
    public uint? TtlInSeconds { get; set; } = ttlInSeconds == 0 ? null! : ttlInSeconds;
}

[AttributeUsage(AttributeTargets.Class)]
public class CachedByUserAttribute(string queryCacheKeyPrefix,
    uint ttlInSeconds = 0) : CachedAttribute(queryCacheKeyPrefix, ttlInSeconds);

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class CacheKeyAttribute : Attribute
{
    
}

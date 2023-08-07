namespace Chatify.Application.Common.Behaviours.Caching;

[AttributeUsage(AttributeTargets.Class)]
public class CachedAttribute : Attribute
{
    public string QueryCacheKeyPrefix { get; set; }
    public uint? TtlInSeconds { get; set; }

    public CachedAttribute(string queryCacheKeyPrefix, uint ttlInSeconds = 0)
    {
        QueryCacheKeyPrefix = queryCacheKeyPrefix;
        TtlInSeconds = ttlInSeconds == 0 ? null! : ttlInSeconds;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class CachedByUserAttribute : CachedAttribute
{
    public CachedByUserAttribute(string queryCacheKeyPrefix, uint ttlInSeconds = 0)
        : base(queryCacheKeyPrefix, ttlInSeconds) { }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class CacheKeyAttribute : Attribute
{
    
}

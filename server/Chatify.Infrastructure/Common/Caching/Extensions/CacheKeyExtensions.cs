using StackExchange.Redis;

namespace Chatify.Infrastructure.Common.Caching.Extensions;

public static class CacheKeyExtensions
{
    public static RedisKey GetUserFeedKey(this Guid userId)
        => new($"user:{userId.ToString()}:feed");
    
    public static RedisKey GetUserFriendsKey(this Guid userId)
        => new($"user:{userId.ToString()}:friends");
    
    public static RedisKey GetUserReactionsKey(this Guid userId)
        => new($"user:{userId.ToString()}:reactions");
    
    public static RedisKey GetMessageReactionsKey(this Guid messageId)
        => new($"message:{messageId.ToString()}:reactions");
    
    public static RedisKey GetGroupMembersKey(this Guid groupId)
        => new($"groups:{groupId.ToString()}:members");
}
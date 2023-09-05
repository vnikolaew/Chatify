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

public static class RedisOperationExtensions
{
    public static Task<RedisValue[]> GetGroupMembersAsync(
        this IDatabase cache,
        Guid groupId)
        => cache.SetMembersAsync(groupId.GetGroupMembersKey());

    public static Task<bool> AddGroupMemberAsync(
        this IDatabase cache,
        Guid groupId,
        Guid userId
    )
        => cache.SetAddAsync(groupId.GetGroupMembersKey(), userId.ToString());
    
    public static Task<bool> RemoveGroupMemberAsync(
        this IDatabase cache,
        Guid groupId,
        Guid userId
    )
        => cache.SetRemoveAsync(groupId.GetGroupMembersKey(), userId.ToString());

    public static Task<RedisValue[]> GetMessageReactionsAsync(
        this IDatabase cache,
        Guid messageId)
        => cache.SetMembersAsync(messageId.GetMessageReactionsKey());

    public static Task<RedisValue[]> GetUserFeedAsync(
        this IDatabase cache,
        Guid userId,
        int offset,
        int limit
    )
        => cache.SortedSetRangeByRankAsync(
            userId.GetUserFeedKey(),
            offset, limit + offset, Order.Descending
        );

    public static Task<bool> AddUserFeedEntryAsync(
        this IDatabase cache,
        Guid userId,
        Guid groupId,
        DateTimeOffset timestamp
    )
        => cache.SortedSetAddAsync(
            userId.GetUserFeedKey(),
            new RedisValue(groupId.ToString()),
            timestamp.Ticks);
    
    public static Task<bool> RemoveUserFeedEntryAsync(
        this IDatabase cache,
        Guid userId,
        Guid groupId
    )
        => cache.SortedSetRemoveAsync(
            userId.GetUserFeedKey(),
            groupId.ToString());

    public static Task<RedisValue[]> GetUserFriendsAsync(
        this IDatabase cache,
        Guid userId)
        => cache.SortedSetRangeByScoreAsync(userId.GetUserFriendsKey(), order: Order.Descending);

    public static Task<RedisValue[]> GetUserReactionsAsync(
        this IDatabase cache,
        Guid userId,
        RedisValue[] fields)
        => cache.HashGetAsync(userId.GetUserReactionsKey(), fields);

    public static Task<bool> AddUserReactionAsync(
        this IDatabase cache,
        Guid userId,
        Guid messageId,
        long reactionCode)
        => cache.HashSetAsync(
            userId.GetUserReactionsKey(),
            new RedisValue(messageId.ToString()),
            new RedisValue(reactionCode.ToString()));

    public static Task<bool> RemoveUserReactionAsync(
        this IDatabase cache,
        Guid userId,
        Guid messageId
    )
        => cache.HashDeleteAsync(
            userId.GetUserReactionsKey(),
            new RedisValue(messageId.ToString())
        );

    public static Task<bool> AddUserFriendAsync(
        this IDatabase cache,
        Guid userId,
        Guid friendId,
        DateTimeOffset timestamp
    )
        => cache.SortedSetAddAsync(
            userId.GetUserFriendsKey(),
            friendId.ToString(),
            timestamp.Ticks,
            SortedSetWhen.NotExists
        );

    public static Task<bool> RemoveUserFriendAsync(
        this IDatabase cache,
        Guid userId,
        Guid friendId
    )
        => cache.SortedSetRemoveAsync(
            userId.GetUserFriendsKey(),
            friendId.ToString());
}
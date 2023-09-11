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
    public static async Task<Guid[]> GetGroupMembersAsync(
        this IDatabase cache,
        Guid groupId)
        => ( await cache.SetMembersAsync(groupId.GetGroupMembersKey()) )
            .Select(_ => Guid.Parse(_.ToString()))
            .ToArray();

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

    public static async Task<Guid[]> GetMessageReactionsAsync(
        this IDatabase cache,
        Guid messageId)
        => ( await cache.SetMembersAsync(messageId.GetMessageReactionsKey()) )
            .Select(_ => Guid.Parse(_.ToString()))
            .ToArray();

    public static async Task<Guid[]> GetUserFeedAsync(
        this IDatabase cache,
        Guid userId,
        int offset,
        int limit
    )
        => ( await cache.SortedSetRangeByRankAsync(
                userId.GetUserFeedKey(),
                offset, limit + offset, Order.Descending
            ) )
            .Select(_ => Guid.Parse(_.ToString()))
            .ToArray();

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

    public static async Task<Guid[]> GetUserFriendsAsync(
        this IDatabase cache,
        Guid userId)
        => ( await cache.SortedSetRangeByScoreAsync(
                userId.GetUserFriendsKey(), order: Order.Descending) )
            .Select(_ => Guid.Parse(_.ToString()))
            .ToArray();

    public static async Task<long?[]> GetUserReactionsAsync(
        this IDatabase cache,
        Guid userId,
        RedisValue[] fields)
        => ( await cache.HashGetAsync(userId.GetUserReactionsKey(), fields) )
            .Select<RedisValue, long?>(r => r.HasValue && r.TryParse(out long value) ? value : default)
            .ToArray();

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
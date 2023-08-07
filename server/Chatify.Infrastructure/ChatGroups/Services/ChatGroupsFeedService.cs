using AutoMapper;
using Chatify.Application.ChatGroups.Contracts;
using Chatify.Application.ChatGroups.Queries;
using Chatify.Domain.Repositories;
using StackExchange.Redis;

namespace Chatify.Infrastructure.ChatGroups.Services;

internal sealed class ChatGroupsFeedService : IChatGroupsFeedService
{
    private readonly IDatabase _cache;
    private readonly IChatMessageRepository _messages;
    private readonly IUserRepository _users;
    private readonly IChatGroupRepository _groups;

    public ChatGroupsFeedService(
        IDatabase cache,
        IChatMessageRepository messages,
        IChatGroupRepository groups,
        IUserRepository users)
    {
        _cache = cache;
        _messages = messages;
        _groups = groups;
        _users = users;
    }

    private static RedisKey GetUserFeedCacheKey(Guid userId)
        => new($"user:{userId}:feed");
    
    public async Task<List<ChatGroupFeedEntry>> GetFeedEntriesForUserAsync(
        Guid userId,
        int limit,
        int offset,
        CancellationToken cancellationToken = default)
    {
        // Get feed with group ids from cache sorted set:
        var userFeedCacheKey = GetUserFeedCacheKey(userId);
        var values = await _cache.SortedSetRangeByRankAsync(
            userFeedCacheKey,
            offset, limit + offset, Order.Descending);

        var groupIds = values
            .Select(v => Guid.Parse(v.ToString()))
            .ToList();

        var groups = await _groups.GetByIds(groupIds, cancellationToken);

        // Query DB to get last message for each Chat Group:
        var messages = await _messages
            .GetLatestForGroups(groupIds, cancellationToken);

        // Query cache for Message Sender info:
        var messageSenderIds = messages.Select(m => m.Value.UserId);
        var userInfos = await _users.GetByIds(messageSenderIds, cancellationToken);

        // Now merge results into a single "Feed" entry:
        return messages
            .Values
            .Select(m => new ChatGroupFeedEntry(
                groups.FirstOrDefault(g => g.Id == m.ChatGroupId)!,
                userInfos!.FirstOrDefault(u => u.Id == m.UserId)!,
                m))
            .OrderByDescending(m => m.ChatMessage.CreatedAt)
            .ToList();
    }
}
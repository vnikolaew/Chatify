using Chatify.Domain.Events.Messages;
using Chatify.Infrastructure.Common.Caching.Extensions;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Client;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace Chatify.Infrastructure.Messages.EventHandlers;

internal sealed class ChatMessageSentEventHandler
    : IEventHandler<ChatMessageSentEvent>
{
    private readonly IHubContext<ChatifyHub, IChatifyHubClient> _chatifyHubContext;
    private readonly IIdentityContext _identityContext;
    private readonly IDatabase _cache;

    public ChatMessageSentEventHandler(
        IHubContext<ChatifyHub, IChatifyHubClient> chatifyHubContext,
        IIdentityContext identityContext,
        IDatabase cache)
    {
        _chatifyHubContext = chatifyHubContext;
        _identityContext = identityContext;
        _cache = cache;
    }

    private static string GetGroupMembersCacheKey(Guid groupId)
        => $"groups:{groupId.ToString()}:members";

    public async Task HandleAsync(
        ChatMessageSentEvent @event,
        CancellationToken cancellationToken = default)
    {
        // Update user caches that serve for feed generation:
        var groupKey = GetGroupMembersCacheKey(@event.GroupId);
        var membersIds = await _cache.SetMembersAsync<Guid>(groupKey);
        foreach (var membersId in membersIds)
        {
            // Update User Feed (Sorted Set):
            var userFeedCacheKey = new RedisKey($"user:{membersId}:feed");
            await _cache.SortedSetAddAsync(
                userFeedCacheKey,
                new RedisValue(
                    @event.GroupId.ToString()
                ), @event.Timestamp.Ticks);
        }

        var groupId = $"chat-groups:{@event.GroupId}";
        await _chatifyHubContext
            .Clients
            .Group(groupId)
            .ReceiveGroupChatMessage(
                new ReceiveGroupChatMessage(
                    @event.GroupId,
                    @event.UserId,
                    @event.MessageId,
                    _identityContext.Username,
                    @event.Content,
                    @event.Timestamp));
    }
}
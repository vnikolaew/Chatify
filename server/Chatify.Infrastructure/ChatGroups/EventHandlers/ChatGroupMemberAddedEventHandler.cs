using Chatify.Application.Common.Contracts;
using Chatify.Domain.Common;
using Chatify.Domain.Events.Groups;
using Chatify.Infrastructure.Data.Models;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using ChatGroup = Chatify.Domain.Entities.ChatGroup;

namespace Chatify.Infrastructure.ChatGroups.EventHandlers;

internal sealed class ChatGroupMemberAddedEventHandler(
        IDatabase cache,
        ILogger<ChatGroupMemberAddedEventHandler> logger,
        IDomainRepository<ChatGroup, Guid> groups,
        ICounterService<ChatGroupMembersCount, Guid> membersCounts,
        IHubContext<ChatifyHub, IChatifyHubClient> chatifyHubContext)
    : IEventHandler<ChatGroupMemberAddedEvent>
{
    private static RedisKey GetGroupMembersCacheKey(Guid groupId)
        => new($"groups:{groupId.ToString()}:members");

    private static RedisKey GetUserFeedCacheKey(Guid userId)
        => new($"user:{userId.ToString()}:feed");

    public async Task HandleAsync(
        ChatGroupMemberAddedEvent @event,
        CancellationToken cancellationToken = default)
    {
        var membersCount = await membersCounts.Increment(@event.GroupId, cancellationToken: cancellationToken);
        logger.LogInformation("Incremented Membership count for Chat Group with Id '{Id}' to {Count} ",
            @event.GroupId, membersCount?.MembersCount);

        // // Add new member to cache set as well:
        var groupMembersCacheKey = GetGroupMembersCacheKey(@event.GroupId);

        // Add user to groups:id:members
        await cache.SetAddAsync(
            groupMembersCacheKey,
            @event.MemberId.ToString());

        // Add group to users:id:feed
        await cache.SortedSetAddAsync(
            GetUserFeedCacheKey(@event.MemberId),
            new RedisValue(@event.GroupId.ToString()),
            @event.Timestamp.Ticks);

        await chatifyHubContext
            .Clients
            .Group(ChatifyHub.GetChatGroupId(@event.GroupId))
            .AddedToChatGroup(new AddedToChatGroup(
                @event.GroupId,
                @event.AddedById,
                @event.AddedByUsername,
                @event.Timestamp));
    }
}
using Chatify.Application.Common.Contracts;
using Chatify.Domain.Common;
using Chatify.Domain.Events.Groups;
using Chatify.Infrastructure.Common.Caching.Extensions;
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
    public async Task HandleAsync(
        ChatGroupMemberAddedEvent @event,
        CancellationToken cancellationToken = default)
    {
        var membersCount = await membersCounts.Increment(@event.GroupId, cancellationToken: cancellationToken);
        logger.LogInformation("Incremented Membership count for Chat Group with Id '{Id}' to {Count} ",
            @event.GroupId, membersCount?.MembersCount);

        // Add user to groups:id:members
        await cache.AddGroupMemberAsync(@event.GroupId, @event.MemberId);

        // Add group to users:id:feed
        await cache.AddUserFeedEntryAsync(
            @event.MemberId,
            @event.GroupId,
            @event.Timestamp
        );

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
using Chatify.Application.Common.Contracts;
using Chatify.Domain.Events.Groups;
using Chatify.Infrastructure.Common.Caching.Extensions;
using Chatify.Infrastructure.Data.Models;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace Chatify.Infrastructure.ChatGroups.EventHandlers;

internal sealed class ChatGroupMemberLeftHandler(
    ICounterService<ChatGroupMembersCount, Guid> memberCounts,
    IDatabase cache,
    IHubContext<ChatifyHub, IChatifyHubClient> chatifyHubContext,
    IIdentityContext identityContext)
    : IEventHandler<ChatGroupMemberLeftEvent>
{
    public async Task HandleAsync(
        ChatGroupMemberLeftEvent @event,
        CancellationToken cancellationToken = default)
    {
        await (
            memberCounts.Decrement(@event.GroupId, cancellationToken: cancellationToken),
            cache.RemoveUserFeedEntryAsync(@event.UserId, @event.GroupId)
        );

        await chatifyHubContext
            .Clients
            .Group(ChatifyHub.GetChatGroupId(@event.GroupId))
            .ChatGroupMemberLeft(new ChatGroupMemberLeft(
                @event.GroupId,
                @event.UserId,
                identityContext.Username,
                @event.Timestamp));
    }
}
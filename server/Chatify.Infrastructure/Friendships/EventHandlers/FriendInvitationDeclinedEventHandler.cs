using Chatify.Domain.Events.Friendships;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.Friendships.EventHandlers;

internal sealed class FriendInvitationDeclinedEventHandler
    (IHubContext<ChatifyHub, IChatifyHubClient> hubContext) : IEventHandler<FriendInvitationDeclinedEvent>
{
    public Task HandleAsync(
        FriendInvitationDeclinedEvent @event,
        CancellationToken cancellationToken = default)
        => hubContext
            .Clients
            .User(@event.InviterId.ToString())
            .FriendInvitationDeclined(new FriendInvitationDeclined(
                @event.InviteeId,
                @event.Timestamp));
}
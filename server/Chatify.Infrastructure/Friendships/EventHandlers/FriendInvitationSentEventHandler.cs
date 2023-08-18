using Chatify.Domain.Events.Friendships;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.Friendships.EventHandlers;

internal sealed class FriendInvitationSentEventHandler
    (IHubContext<ChatifyHub, IChatifyHubClient> hubContext) : IEventHandler<FriendInvitationSentEvent>
{
    public Task HandleAsync(FriendInvitationSentEvent @event, CancellationToken cancellationToken = default)
        => hubContext
            .Clients
            .User(@event.InviteeId.ToString())
            .ReceiveFriendInvitation(new ReceiveFriendInvitation(
                @event.InviterId,
                @event.Timestamp));
}
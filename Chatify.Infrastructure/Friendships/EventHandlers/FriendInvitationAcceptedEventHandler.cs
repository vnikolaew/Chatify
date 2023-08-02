using Chatify.Domain.Events.Friendships;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.Friendships.EventHandlers;

internal sealed class FriendInvitationAcceptedEventHandler : IEventHandler<FriendInvitationAcceptedEvent>
{
    private readonly IHubContext<ChatifyHub, IChatifyHubClient> _hubContext;

    public FriendInvitationAcceptedEventHandler(
        IHubContext<ChatifyHub, IChatifyHubClient> hubContext) =>
        _hubContext = hubContext;

    public Task HandleAsync(FriendInvitationAcceptedEvent @event, CancellationToken cancellationToken = default)
        => _hubContext
            .Clients
            .User(@event.InviterId.ToString())
            .FriendInvitationAccepted(new FriendInvitationAccepted(
                @event.InviteeId,
                @event.Timestamp));
}
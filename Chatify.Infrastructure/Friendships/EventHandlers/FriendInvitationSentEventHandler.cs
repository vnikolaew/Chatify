using Chatify.Domain.Events.Friendships;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.Friendships.EventHandlers;

internal sealed class FriendInvitationSentEventHandler : IEventHandler<FriendInvitationSentEvent>
{
    private readonly IHubContext<ChatifyHub, IChatifyHubClient> _hubContext;

    public FriendInvitationSentEventHandler(
        IHubContext<ChatifyHub, IChatifyHubClient> hubContext)
        => _hubContext = hubContext;

    public Task HandleAsync(FriendInvitationSentEvent @event, CancellationToken cancellationToken = default)
        => _hubContext
            .Clients
            .User(@event.InviteeId.ToString())
            .ReceiveFriendInvitation(new ReceiveFriendInvitation(
                @event.InviterId,
                @event.Timestamp));
}
using Chatify.Domain.Events.Friendships;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.Friendships.EventHandlers;

internal sealed class FriendInvitationDeclinedEventHandler
    : IEventHandler<FriendInvitationDeclinedEvent>
{
    private readonly IHubContext<ChatifyHub, IChatifyHubClient> _hubContext;

    public FriendInvitationDeclinedEventHandler(
        IHubContext<ChatifyHub, IChatifyHubClient> hubContext) =>
        _hubContext = hubContext;

    public Task HandleAsync(
        FriendInvitationDeclinedEvent @event,
        CancellationToken cancellationToken = default)
        => _hubContext
            .Clients
            .User(@event.InviterId.ToString())
            .FriendInvitationDeclined(new FriendInvitationDeclined(
                @event.InviteeId,
                @event.Timestamp));
}
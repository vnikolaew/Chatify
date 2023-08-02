using Chatify.Domain.Entities;
using Chatify.Domain.Events.Users;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.User.EventHandlers;

internal sealed class UserChangedStatusEventHandler
    : IEventHandler<UserChangedStatusEvent>
{
    private readonly IHubContext<ChatifyHub, IChatifyHubClient> _hubContext;
    private readonly IFriendshipsRepository _friends;
    private readonly IIdentityContext _identityContext;

    public UserChangedStatusEventHandler(
        IHubContext<ChatifyHub, IChatifyHubClient> hubContext,
        IFriendshipsRepository friends,
        IIdentityContext identityContext)
    {
        _friends = friends;
        _identityContext = identityContext;
        _hubContext = hubContext;
    }

    public async Task HandleAsync(
        UserChangedStatusEvent @event,
        CancellationToken cancellationToken = default)
    {
        var friendsRelations = await _friends.AllForUser(@event.UserId);
        var userIds = friendsRelations
            .Select(f => f.FriendTwoId.ToString());

        await _hubContext
            .Clients
            .Users(userIds)
            .UserStatusChanged(
                new UserStatusChanged(
                    @event.UserId,
                    _identityContext.Username,
                    (sbyte) @event.NewStatus,
                    @event.Timestamp));
    }
}
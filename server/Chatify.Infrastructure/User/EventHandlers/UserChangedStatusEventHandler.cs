using Chatify.Domain.Events.Users;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.User.EventHandlers;

internal sealed class UserChangedStatusEventHandler(IHubContext<ChatifyHub, IChatifyHubClient> hubContext,
        IFriendshipsRepository friends,
        IIdentityContext identityContext)
    : IEventHandler<UserChangedStatusEvent>
{
    public async Task HandleAsync(
        UserChangedStatusEvent @event,
        CancellationToken cancellationToken = default)
    {
        var friendIds = await friends.AllFriendIdsForUser(@event.UserId, cancellationToken);

        await hubContext
            .Clients
            .Users(friendIds.Select(_ => _.ToString()))
            .UserStatusChanged(
                new UserStatusChanged(
                    @event.UserId,
                    identityContext.Username,
                    (sbyte) @event.NewStatus,
                    @event.Timestamp));
    }
}
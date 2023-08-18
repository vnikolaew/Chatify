using Chatify.Domain.Events.JoinRequests;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.JoinRequests.EventHandlers;

internal sealed class ChatGroupJoinRequestedEventHandler(IHubContext<ChatifyHub, IChatifyHubClient> chatifyContext,
        IChatGroupRepository groups, IUserRepository users)
    : IEventHandler<ChatGroupJoinRequested>
{
    public async Task HandleAsync(
        ChatGroupJoinRequested @event,
        CancellationToken cancellationToken = default)
    {
        var group = await groups.GetAsync(@event.GroupId, cancellationToken);
        if ( group is null ) return;

        var user = await users.GetAsync(@event.UserId, cancellationToken);
        if ( user is null ) return;

        // Notify group admins for user request:
        await chatifyContext
            .Clients
            .Users(group.AdminIds.Select(_ => _.ToString()))
            .ChatGroupJoinRequested(new ChatGroupUserJoinRequested(
                    group.Id,
                    user.Id,
                    user.Username,
                    user.ProfilePicture.MediaUrl,
                    @event.Timestamp
                )
            );
    }
}
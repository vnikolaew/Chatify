using Chatify.Domain.Events.JoinRequests;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.JoinRequests.EventHandlers;

internal sealed class ChatGroupJoinRequestAcceptedEventHandler(
        IHubContext<ChatifyHub, IChatifyHubClient> chatifyContext,
        IUserRepository users)
    : IEventHandler<ChatGroupJoinRequestAccepted>
{
    public async Task HandleAsync(
        ChatGroupJoinRequestAccepted @event,
        CancellationToken cancellationToken = default)
    {
        var adminUser = await users.GetAsync(@event.AcceptedById, cancellationToken);
        if ( adminUser is null ) return;

        await chatifyContext
            .Clients
            .User(@event.UserId.ToString())
            .ChatGroupJoinRequestAccepted(new ChatGroupUserJoinRequestAccepted(
                @event.GroupId,
                @event.UserId,
                adminUser.Id,
                adminUser.Username,
                adminUser.ProfilePicture.MediaUrl,
                @event.Timestamp));
    }
}
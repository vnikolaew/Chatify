using Chatify.Domain.Events.JoinRequests;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.JoinRequests.EventHandlers;

internal sealed class ChatGroupJoinRequestDeclinedEventHandler(
        IHubContext<ChatifyHub, IChatifyHubClient> chatifyContext,
        IUserRepository users, IChatGroupRepository groups)
    : IEventHandler<ChatGroupJoinRequestDeclined>
{
    private readonly IChatGroupRepository _groups = groups;

    public async Task HandleAsync(ChatGroupJoinRequestDeclined @event, CancellationToken cancellationToken = default)
    {
        var adminUser = await users.GetAsync(@event.DeclinedById, cancellationToken);
        if ( adminUser is null ) return;

        await chatifyContext
            .Clients
            .User(@event.UserId.ToString())
            .ChatGroupUserJoinRequestDeclined(new ChatGroupUserJoinRequestDeclined(
                @event.GroupId,
                @event.UserId,
                adminUser.Id,
                adminUser.Username,
                adminUser.ProfilePicture.MediaUrl,
                @event.Timestamp));
    }
}
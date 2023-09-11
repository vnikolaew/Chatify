using Chatify.Domain.Events.Groups;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.ChatGroups.EventHandlers;

internal sealed class ChatGroupAdminAddedHandler
(IHubContext<ChatifyHub, IChatifyHubClient> hubContext,
    IUserRepository users
) : IEventHandler<ChatGroupAdminAdded>
{
    public async Task HandleAsync(ChatGroupAdminAdded @event,
        CancellationToken cancellationToken = default)
    {
        var newAdmin = await users.GetAsync(@event.AdminId, cancellationToken);

        await hubContext
            .Clients
            .Group(ChatifyHub.GetChatGroupId(@event.GroupId))
            .ChatGroupNewAdminAdded(new ChatGroupNewAdminAdded(
                @event.GroupId,
                @event.AdminId,
                newAdmin!.Username,
                @event.Timestamp));
    }
}
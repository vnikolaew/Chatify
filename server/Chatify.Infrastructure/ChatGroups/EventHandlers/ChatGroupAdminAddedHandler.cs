using Chatify.Domain.Events.Groups;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.ChatGroups.EventHandlers;

internal sealed class ChatGroupAdminAddedHandler : IEventHandler<ChatGroupAdminAdded>
{
    private readonly IHubContext<ChatifyHub, IChatifyHubClient> _hubContext;

    public ChatGroupAdminAddedHandler(
        IHubContext<ChatifyHub, IChatifyHubClient> hubContext)
        => _hubContext = hubContext;

    public async Task HandleAsync(ChatGroupAdminAdded @event, CancellationToken cancellationToken = default)
    {
        var groupId = $"chat-groups:{@event.GroupId}";
        
        await _hubContext
            .Clients
            .Group(groupId)
            .ChatGroupNewAdminAdded(new ChatGroupNewAdminAdded(
                @event.GroupId,
                @event.AdminId,
                @event.Timestamp));
    }
}
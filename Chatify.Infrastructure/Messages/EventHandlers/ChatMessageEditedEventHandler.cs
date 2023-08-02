using Chatify.Domain.Events.Messages;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.Messages.EventHandlers;

internal sealed class ChatMessageEditedEventHandler
    : IEventHandler<ChatMessageEditedEvent>
{
    private readonly IHubContext<ChatifyHub, IChatifyHubClient> _hubContext;

    public ChatMessageEditedEventHandler(
        IHubContext<ChatifyHub, IChatifyHubClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task HandleAsync(
        ChatMessageEditedEvent @event,
        CancellationToken cancellationToken = default)
    {
        var groupId = $"chat-groups:{@event.GroupId}";
        await  _hubContext
            .Clients
            .Group(groupId)
            .ChatGroupMessageEdited(
                new ChatGroupMessageEdited(
                    @event.GroupId,
                    @event.MessageId,
                    @event.UserId,
                    @event.NewContent,
                    @event.Timestamp));
    }
}
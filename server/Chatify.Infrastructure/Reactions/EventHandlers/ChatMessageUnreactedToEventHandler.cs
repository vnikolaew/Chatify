using Chatify.Domain.Events.Messages;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.Reactions.EventHandlers;

internal sealed class ChatMessageUnreactedToEventHandler
    : IEventHandler<ChatMessageUnreactedToEvent>
{
    private readonly IHubContext<ChatifyHub, IChatifyHubClient> _hubContext;

    public ChatMessageUnreactedToEventHandler(IHubContext<ChatifyHub, IChatifyHubClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task HandleAsync(
        ChatMessageUnreactedToEvent @event,
        CancellationToken cancellationToken = default)
    {
        var groupId = $"chat-groups:{@event.GroupId}";
        await _hubContext
            .Clients
            .Group(groupId)
            .ChatGroupMessageUnReactedTo(
                new ChatGroupMessageUnReactedTo(
                    @event.GroupId,
                    @event.MessageId,
                    @event.MessageReactionId,
                    @event.UserId,
                    @event.ReactionType,
                    @event.Timestamp));
    }
}
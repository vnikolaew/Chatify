using Chatify.Domain.Events.Messages;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.Reactions.EventHandlers;

internal sealed class ChatMessageReactedToEventHandler
    : IEventHandler<ChatMessageReactedToEvent>
{
    private readonly IHubContext<ChatifyHub, IChatifyHubClient> _hubContext;

    public ChatMessageReactedToEventHandler(
        IHubContext<ChatifyHub, IChatifyHubClient> hubContext)
        => _hubContext = hubContext;

    public async Task HandleAsync(
        ChatMessageReactedToEvent @event,
        CancellationToken cancellationToken = default)
    {
        var groupId = $"chat-groups:{@event.GroupId}";
        await _hubContext
            .Clients
            .Group(groupId)
            .ChatGroupMessageReactedTo(
                new ChatGroupMessageReactedTo(
                    @event.GroupId,
                    @event.MessageId,
                    @event.MessageReactionId,
                    @event.UserId,
                    @event.ReactionType,
                    @event.Timestamp));
    }
}
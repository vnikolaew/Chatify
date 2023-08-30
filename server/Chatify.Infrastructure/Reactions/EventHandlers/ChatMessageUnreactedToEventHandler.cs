using Chatify.Domain.Events.Messages;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.Reactions.EventHandlers;

internal sealed class ChatMessageUnreactedToEventHandler
    (IHubContext<ChatifyHub, IChatifyHubClient> hubContext) : IEventHandler<ChatMessageUnreactedToEvent>
{
    public async Task HandleAsync(
        ChatMessageUnreactedToEvent @event,
        CancellationToken cancellationToken = default)
    {
        await hubContext
            .Clients
            .Group(ChatifyHub.GetChatGroupId(@event.GroupId))
            .ChatGroupMessageUnReactedTo(
                new ChatGroupMessageUnReactedTo(
                    @event.GroupId,
                    @event.MessageId,
                    @event.MessageReactionId,
                    @event.UserId,
                    @event.ReactionCode,
                    @event.Timestamp));
    }
}
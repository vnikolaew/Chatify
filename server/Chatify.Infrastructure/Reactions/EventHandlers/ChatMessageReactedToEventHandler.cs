using Cassandra.Mapping;
using Chatify.Domain.Events.Messages;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Data.Models;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.Reactions.EventHandlers;

internal sealed class ChatMessageReactedToEventHandler(IHubContext<ChatifyHub, IChatifyHubClient> hubContext,
        IChatMessageReactionRepository reactions, IMapper mapper)
    : IEventHandler<ChatMessageReactedToEvent>
{
    private readonly IChatMessageReactionRepository _reactions = reactions;

    public async Task HandleAsync(
        ChatMessageReactedToEvent @event,
        CancellationToken cancellationToken = default)
    {
        var currentCount = await mapper.FirstOrDefaultAsync<long>(
                $"SELECT reaction_counts[{@event.ReactionType}] FROM message_reactions WHERE message_id = ?;",
                @event.MessageId);

        await mapper.UpdateAsync<ChatMessageReaction>(
            $" SET reaction_counts[{@event.ReactionType}] = ? WHERE message_id = ?",
        currentCount + 1, @event.MessageId);

        await hubContext
            .Clients
            .Group(ChatifyHub.GetChatGroupId(@event.GroupId))
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
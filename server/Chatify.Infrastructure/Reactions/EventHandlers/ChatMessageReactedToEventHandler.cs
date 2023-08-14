using Cassandra.Mapping;
using Chatify.Domain.Events.Messages;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Data.Models;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.Reactions.EventHandlers;

internal sealed class ChatMessageReactedToEventHandler
    : IEventHandler<ChatMessageReactedToEvent>
{
    private readonly IHubContext<ChatifyHub, IChatifyHubClient> _hubContext;
    private readonly IChatMessageReactionRepository _reactions;
    private readonly IMapper _mapper;

    public ChatMessageReactedToEventHandler(
        IHubContext<ChatifyHub, IChatifyHubClient> hubContext,
        IChatMessageReactionRepository reactions, IMapper mapper)
    {
        _hubContext = hubContext;
        _reactions = reactions;
        _mapper = mapper;
    }

    public async Task HandleAsync(
        ChatMessageReactedToEvent @event,
        CancellationToken cancellationToken = default)
    {
        var currentCount = await _mapper.FirstOrDefaultAsync<long>(
                $"SELECT reaction_counts[{@event.ReactionType}] FROM message_reactions WHERE message_id = ?;",
                @event.MessageId);

        await _mapper.UpdateAsync<ChatMessageReaction>(
            $" SET reaction_counts[{@event.ReactionType}] = ? WHERE message_id = ?",
        currentCount + 1, @event.MessageId);

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
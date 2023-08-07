using Cassandra.Mapping;
using Chatify.Domain.Events.Messages;
using Chatify.Infrastructure.Data.Models;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.Messages.EventHandlers;

internal sealed class ChatMessageEditedEventHandler
    : IEventHandler<ChatMessageEditedEvent>
{
    private readonly IHubContext<ChatifyHub, IChatifyHubClient> _hubContext;
    private readonly IMapper _mapper;

    public ChatMessageEditedEventHandler(
        IHubContext<ChatifyHub, IChatifyHubClient> hubContext, IMapper mapper)
    {
        _hubContext = hubContext;
        _mapper = mapper;
    }

    public async Task HandleAsync(
        ChatMessageEditedEvent @event,
        CancellationToken cancellationToken = default)
    {
        // Update Message Reply Summaries "View" table:
        await _mapper.UpdateAsync<ChatMessageRepliesSummary>(
            "SET updated_at = ?, updated = ? WHERE message_id = ? ALLOW FILTERING;",
            @event.Timestamp,
            true,
            @event.MessageId
        );
        
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
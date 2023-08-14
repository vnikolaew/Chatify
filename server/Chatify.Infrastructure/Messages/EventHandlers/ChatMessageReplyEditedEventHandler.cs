using Cassandra.Mapping;
using Chatify.Domain.Events.Messages;
using Chatify.Infrastructure.Common.Extensions;
using Chatify.Infrastructure.Data.Models;
using Chatify.Infrastructure.Messages.BackgroundJobs;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;
using Quartz;

namespace Chatify.Infrastructure.Messages.EventHandlers;

internal sealed class ChatMessageReplyEditedEventHandler
    : IEventHandler<ChatMessageReplyEditedEvent >
{
    private readonly IHubContext<ChatifyHub, IChatifyHubClient> _hubContext;
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IMapper _mapper;

    public ChatMessageReplyEditedEventHandler(
        IHubContext<ChatifyHub, IChatifyHubClient> hubContext,
        ISchedulerFactory schedulerFactory,
        IMapper mapper)
    {
        _hubContext = hubContext;
        _schedulerFactory = schedulerFactory;
        _mapper = mapper;
    }

    public async Task HandleAsync(
        ChatMessageReplyEditedEvent @event,
        CancellationToken cancellationToken = default)
    {
        // Update Message Reply Summaries "View" table:
        await _mapper.UpdateAsync<ChatMessageRepliesSummary>(
            "SET updated_at = ?, updated = ? WHERE message_id = ? ALLOW FILTERING;",
            @event.Timestamp,
            true,
            @event.ReplyToId
        );
        
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        await scheduler.ScheduleImmediateJob<ProcessChatMessageReplyJob>(builder =>
            builder.WithMessageId(@event.MessageId), cancellationToken);
        
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
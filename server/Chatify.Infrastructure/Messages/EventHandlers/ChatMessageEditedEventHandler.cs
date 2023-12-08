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
using Quartz.Impl;

namespace Chatify.Infrastructure.Messages.EventHandlers;

internal sealed class ChatMessageEditedEventHandler(IHubContext<ChatifyHub, IChatifyHubClient> hubContext,
        IMapper mapper,
        ISchedulerFactory schedulerFactory)
    : IEventHandler<ChatMessageEditedEvent>
{
    public async Task HandleAsync(
        ChatMessageEditedEvent @event,
        CancellationToken cancellationToken = default)
    {
        // Update Message Reply Summaries "View" table:
        await mapper.UpdateAsync<ChatMessageRepliesSummary>(
            "SET updated_at = ?, updated = ? WHERE message_id = ? ALLOW FILTERING;",
            @event.Timestamp,
            true,
            @event.MessageId
        );
        
        var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
        await scheduler.ScheduleImmediateJob<ProcessChatMessageJob>(builder =>
            builder.WithMessageId(@event.MessageId), cancellationToken);
        
        await  hubContext
            .Clients
            .Group(ChatifyHub.GetChatGroupId(@event.GroupId))
            .ChatGroupMessageEdited(
                new ChatGroupMessageEdited(
                    @event.GroupId,
                    @event.MessageId,
                    @event.UserId,
                    @event.NewContent,
                    @event.Timestamp));
    }
}
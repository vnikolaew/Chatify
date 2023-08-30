using Cassandra.Mapping;
using Chatify.Application.Common.Contracts;
using Chatify.Domain.Events.Messages;
using Chatify.Infrastructure.Common.Extensions;
using Chatify.Infrastructure.Data.Models;
using Chatify.Infrastructure.Messages.BackgroundJobs;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Client;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;
using Quartz;

namespace Chatify.Infrastructure.Messages.EventHandlers;

internal sealed class ChatMessageRepliedToEventHandler(ICounterService<ChatMessageReplyCount, Guid> replyCounts,
        IHubContext<ChatifyHub, IChatifyHubClient> chatifyContext,
        IIdentityContext identityContext,
        IMapper mapper, ISchedulerFactory schedulerFactory)
    : IEventHandler<ChatMessageRepliedToEvent>
{
    public async Task HandleAsync(
        ChatMessageRepliedToEvent @event,
        CancellationToken cancellationToken = default)
    {
        await replyCounts.Increment(
            @event.ReplyToId,
            cancellationToken: cancellationToken);

        // Update Message Reply Summaries "View" table:
        var replierIds = await mapper.FirstOrDefaultAsync<HashSet<Guid>>(
            "SELECT replier_ids FROM chat_message_replies_summaries WHERE message_id = ? ALLOW FILTERING;",
            @event.MessageId);
        if ( replierIds is not null )
        {
            if ( !replierIds.Contains(@event.UserId) )
            {
                var user = await mapper.FirstOrDefaultAsync<ChatifyUser>(
                    "SELECT id, username, profile_picture FROM users WHERE id = ?;", @event.UserId);

                var userInfo = new MessageReplierInfo
                {
                    UserId = user.Id,
                    Username = user.UserName,
                    ProfilePictureUrl = user.ProfilePicture.MediaUrl
                };

                await mapper.UpdateAsync<ChatMessageRepliesSummary>(
                    " SET replier_ids = replier_ids + ?, updated_at = ?, updated = ?, user_infos = user_infos + ?, total = total + 1 WHERE message_id = ?",
                    user.Id,
                    @event.Timestamp,
                    true,
                    userInfo,
                    @event.MessageId
                );
            }
            else
            {
                await mapper.UpdateAsync<ChatMessageRepliesSummary>(
                    " SET total = total + 1, updated_at = ?, updated = ? WHERE message_id = ?",
                    @event.MessageId,
                    @event.Timestamp,
                    true
                );
            }
        }

        var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
        await scheduler.ScheduleImmediateJob<ProcessChatMessageReplyJob>(builder =>
            builder.WithMessageId(@event.MessageId), cancellationToken);
        
        await chatifyContext
            .Clients
            .Group(ChatifyHub.GetChatGroupId(@event.GroupId))
            .ReceiveGroupChatMessage(
                new ReceiveGroupChatMessage(
                    @event.GroupId,
                    @event.UserId,
                    @event.MessageId,
                    identityContext.Username,
                    @event.Content,
                    @event.Timestamp));
    }
}
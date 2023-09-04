﻿using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Messages;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Caching.Extensions;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Client;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;
using Quartz;
using StackExchange.Redis;
using Guid = System.Guid;

namespace Chatify.Infrastructure.Messages.EventHandlers;

internal sealed class ChatMessageSentEventHandler(
        IHubContext<ChatifyHub, IChatifyHubClient> chatifyHubContext,
        IIdentityContext identityContext,
        IDatabase cache,
        IDomainRepository<MessageRepliersInfo, Guid> replierInfos, ISchedulerFactory schedulerFactory,
        IChatGroupAttachmentRepository attachments, IChatMessageRepository messages)
    : IEventHandler<ChatMessageSentEvent>
{
    public async Task HandleAsync(
        ChatMessageSentEvent @event,
        CancellationToken cancellationToken = default)
    {
        // Update user caches that serve for feed generation:
        var groupKey = @event.GroupId.GetGroupMembersKey();
        var membersIds = await cache.SetMembersAsync(groupKey);
        foreach (var membersId in membersIds.Select(_ => Guid.Parse(_.ToString())))
        {
            // Update User Feed (Sorted Set):
            await cache.SortedSetAddAsync(
                membersId.GetUserFeedKey(),
                new RedisValue(
                    @event.GroupId.ToString()
                ), @event.Timestamp.Ticks);
        }

        // Add a Message Repliers Summary entry to DB:
        var repliersInfo = new MessageRepliersInfo
        {
            Id = Guid.NewGuid(),
            MessageId = @event.MessageId,
            Total = 0,
            ChatGroupId = @event.GroupId,
            ReplierInfos = new HashSet<MessageReplierInfo>()
        };
        await replierInfos.SaveAsync(repliersInfo, cancellationToken);
        
        // Handle update of group attachments "View" table:
        var message = await messages
            .GetAsync(@event.MessageId, cancellationToken);
        
        var groupAttachments = message!
            .Attachments
            .Select(media => new ChatGroupAttachment
            {
                ChatGroupId = message.ChatGroupId,
                UserId = identityContext.Id,
                Username = identityContext.Username,
                CreatedAt = message.CreatedAt.DateTime,
                AttachmentId = media.Id,
                MediaInfo = media
            });
        await attachments.SaveManyAsync(groupAttachments, cancellationToken);

        // var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
        // await scheduler.ScheduleImmediateJob<ProcessChatMessageJob>(builder =>
        //     builder.WithMessageId(@event.MessageId), cancellationToken);
        
        await chatifyHubContext
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
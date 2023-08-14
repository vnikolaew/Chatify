using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Messages;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Caching.Extensions;
using Chatify.Infrastructure.Common.Extensions;
using Chatify.Infrastructure.Messages.BackgroundJobs;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Client;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;
using Quartz;
using StackExchange.Redis;

namespace Chatify.Infrastructure.Messages.EventHandlers;

internal sealed class ChatMessageSentEventHandler
    : IEventHandler<ChatMessageSentEvent>
{
    private readonly IHubContext<ChatifyHub, IChatifyHubClient> _chatifyHubContext;
    private readonly IIdentityContext _identityContext;
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IDomainRepository<MessageRepliersInfo, Guid> _replierInfos;
    private readonly IChatGroupAttachmentRepository _attachments;
    private readonly IChatMessageRepository _messages;
    private readonly IDatabase _cache;

    public ChatMessageSentEventHandler(
        IHubContext<ChatifyHub, IChatifyHubClient> chatifyHubContext,
        IIdentityContext identityContext,
        IDatabase cache,
        IDomainRepository<MessageRepliersInfo, Guid> replierInfos, ISchedulerFactory schedulerFactory, IChatGroupAttachmentRepository attachments, IChatMessageRepository messages)
    {
        _chatifyHubContext = chatifyHubContext;
        _identityContext = identityContext;
        _cache = cache;
        _replierInfos = replierInfos;
        _schedulerFactory = schedulerFactory;
        _attachments = attachments;
        _messages = messages;
    }

    private static string GetGroupMembersCacheKey(Guid groupId)
        => $"groups:{groupId.ToString()}:members";

    public async Task HandleAsync(
        ChatMessageSentEvent @event,
        CancellationToken cancellationToken = default)
    {
        // Update user caches that serve for feed generation:
        var groupKey = GetGroupMembersCacheKey(@event.GroupId);
        var membersIds = await _cache.SetMembersAsync<Guid>(groupKey);
        foreach (var membersId in membersIds)
        {
            // Update User Feed (Sorted Set):
            var userFeedCacheKey = new RedisKey($"user:{membersId}:feed");
            await _cache.SortedSetAddAsync(
                userFeedCacheKey,
                new RedisValue(
                    @event.GroupId.ToString()
                ), @event.Timestamp.Ticks);
        }

        // Add a Message Repliers Summary entry to DB:
        var repliersInfo = new MessageRepliersInfo
        {
            MessageId = @event.MessageId,
            Total = 0,
            ChatGroupId = @event.GroupId,
            ReplierInfos = new HashSet<MessageReplierInfo>(),
        };
        await _replierInfos.SaveAsync(repliersInfo, cancellationToken);
        
        // Handle update of group attachments "View" table:
        var message = await _messages
            .GetAsync(@event.MessageId, cancellationToken);
        
        var groupAttachments = message!
            .Attachments
            .Select(media => new ChatGroupAttachment
            {
                ChatGroupId = message.ChatGroupId,
                UserId = _identityContext.Id,
                Username = _identityContext.Username,
                CreatedAt = message.CreatedAt.DateTime,
                AttachmentId = media.Id,
                MediaInfo = media,
            });
        await _attachments.SaveManyAsync(groupAttachments, cancellationToken);

        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        await scheduler.ScheduleImmediateJob<ProcessChatMessageJob>(builder =>
            builder.WithMessageId(@event.MessageId), cancellationToken);
        
        var groupId = $"chat-groups:{@event.GroupId}";
        await _chatifyHubContext
            .Clients
            .Group(groupId)
            .ReceiveGroupChatMessage(
                new ReceiveGroupChatMessage(
                    @event.GroupId,
                    @event.UserId,
                    @event.MessageId,
                    _identityContext.Username,
                    @event.Content,
                    @event.Timestamp));
    }
}
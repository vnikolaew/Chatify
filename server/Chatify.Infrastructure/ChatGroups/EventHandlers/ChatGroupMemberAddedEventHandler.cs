using Chatify.Application.Common.Contracts;
using Chatify.Domain.Common;
using Chatify.Domain.Events.Groups;
using Chatify.Infrastructure.Data.Models;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Serialization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using ChatGroup = Chatify.Domain.Entities.ChatGroup;

namespace Chatify.Infrastructure.ChatGroups.EventHandlers;

internal sealed class ChatGroupMemberAddedEventHandler
    : IEventHandler<ChatGroupMemberAddedEvent>
{
    private readonly IDomainRepository<ChatGroup, Guid> _groups;
    private readonly ILogger<ChatGroupMemberAddedEventHandler> _logger;
    private readonly ICounterService<ChatGroupMembersCount, Guid> _membersCounts;
    private readonly IHubContext<ChatifyHub, IChatifyHubClient> _chatifyHubContext;
    private readonly IDatabase _cache;

    public ChatGroupMemberAddedEventHandler(
        ILogger<ChatGroupMemberAddedEventHandler> logger,
        IDomainRepository<ChatGroup, Guid> groups,
        ICounterService<ChatGroupMembersCount, Guid> membersCounts,
        IHubContext<ChatifyHub, IChatifyHubClient> chatifyHubContext,
        IDatabase cache)
    {
        _logger = logger;
        _groups = groups;
        _membersCounts = membersCounts;
        _chatifyHubContext = chatifyHubContext;
        _cache = cache;
    }
    
    private static RedisKey GetGroupMembersCacheKey(Guid groupId)
        => new($"groups:{groupId.ToString()}:members");

    public async Task HandleAsync(
        ChatGroupMemberAddedEvent @event,
        CancellationToken cancellationToken = default)
    {
        var group = await _groups.GetAsync(@event.GroupId, cancellationToken);
        if(group is null) return;

        var membersCount = await _membersCounts.Increment(group.Id, cancellationToken: cancellationToken);
        _logger.LogInformation("Incremented Membership count for Chat Group with Id '{Id}' to {Count} ",
            @event.GroupId, membersCount?.MembersCount);
        
        // Add new member to cache set as well:
        var groupKey = GetGroupMembersCacheKey(group.Id);
        var userKey = new RedisValue(@event.MemberId.ToString());
        await _cache.SetAddAsync(groupKey, userKey);
        
        var groupId = $"chat-groups:{@event.GroupId}";
        await _chatifyHubContext
            .Clients
            .Group(groupId)
            .AddedToChatGroup(new AddedToChatGroup(
                @event.GroupId,
                @event.AddedById,
                @event.AddedByUsername,
                @event.Timestamp));
    }
}
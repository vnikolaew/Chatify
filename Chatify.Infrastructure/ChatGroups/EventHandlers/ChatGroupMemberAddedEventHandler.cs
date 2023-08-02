using Chatify.Application.Common.Contracts;
using Chatify.Domain.Common;
using Chatify.Domain.Events.Groups;
using Chatify.Infrastructure.Data.Models;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

using ChatGroup = Chatify.Domain.Entities.ChatGroup;

namespace Chatify.Infrastructure.ChatGroups.EventHandlers;

internal sealed class ChatGroupMemberAddedEventHandler
    : IEventHandler<ChatGroupMemberAddedEvent>
{
    private readonly IDomainRepository<ChatGroup, Guid> _groups;
    private readonly ILogger<ChatGroupMemberAddedEventHandler> _logger;
    private readonly ICounterService<ChatGroupMembersCount, Guid> _membersCounts;
    private readonly IHubContext<ChatifyHub, IChatifyHubClient> _chatifyHubContext;

    public ChatGroupMemberAddedEventHandler(
        ILogger<ChatGroupMemberAddedEventHandler> logger,
        IDomainRepository<ChatGroup, Guid> groups,
        ICounterService<ChatGroupMembersCount, Guid> membersCounts,
        IHubContext<ChatifyHub, IChatifyHubClient> chatifyHubContext)
    {
        _logger = logger;
        _groups = groups;
        _membersCounts = membersCounts;
        _chatifyHubContext = chatifyHubContext;
    }

    public async Task HandleAsync(
        ChatGroupMemberAddedEvent @event,
        CancellationToken cancellationToken = default)
    {
        var group = await _groups.GetAsync(@event.GroupId, cancellationToken);
        if(group is null) return;

        var membersCount = await _membersCounts.Increment(group.Id, cancellationToken: cancellationToken);
        _logger.LogInformation("Incremented Membership count for Chat Group with Id '{Id}' to {Count} ",
            @event.GroupId, membersCount?.MembersCount);
        
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
using Cassandra.Mapping;
using Chatify.Application.Common.Contracts;
using Chatify.Domain.Common;
using Chatify.Domain.Events.Groups;
using Chatify.Infrastructure.Data.Models;
using Chatify.Shared.Abstractions.Events;
using Microsoft.Extensions.Logging;

using ChatGroup = Chatify.Domain.Entities.ChatGroup;

namespace Chatify.Infrastructure.ChatGroups.EventHandlers;

internal sealed class ChatGroupMemberAddedEventHandler : IEventHandler<ChatGroupMemberAddedEvent>
{
    private readonly IDomainRepository<ChatGroup, Guid> _groups;
    private readonly ILogger<ChatGroupMemberAddedEventHandler> _logger;
    private readonly ICounterService<ChatGroupMembersCount, Guid> _membersCounts;

    public ChatGroupMemberAddedEventHandler(
        ILogger<ChatGroupMemberAddedEventHandler> logger,
        IDomainRepository<ChatGroup, Guid> groups,
        ICounterService<ChatGroupMembersCount, Guid> membersCounts)
    {
        _logger = logger;
        _groups = groups;
        _membersCounts = membersCounts;
    }

    public async Task HandleAsync(ChatGroupMemberAddedEvent @event, CancellationToken cancellationToken = default)
    {
        var group = await _groups.GetAsync(@event.GroupId, cancellationToken);
        if(group is null) return;

        var membersCount = await _membersCounts.Increment(group.Id, cancellationToken: cancellationToken);
        _logger.LogInformation("Incremented Membership count for Chat Group with Id '{Id}' to {Count} ",
            @event.GroupId, membersCount?.MembersCount);
    }
}
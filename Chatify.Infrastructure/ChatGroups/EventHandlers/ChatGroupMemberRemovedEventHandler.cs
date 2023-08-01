using Chatify.Application.Common.Contracts;
using Chatify.Domain.Common;
using Chatify.Domain.Events.Groups;
using Chatify.Infrastructure.Data.Models;
using Chatify.Shared.Abstractions.Events;
using Microsoft.Extensions.Logging;

namespace Chatify.Infrastructure.ChatGroups.EventHandlers;

internal sealed class ChatGroupMemberRemovedEventHandler
    : IEventHandler<ChatGroupMemberRemovedEvent>
{
    private readonly IDomainRepository<ChatGroup, Guid> _groups;
    private readonly ILogger<ChatGroupMemberRemovedEventHandler> _logger;
    private readonly ICounterService<ChatGroupMembersCount, Guid> _membersCounts;

    public ChatGroupMemberRemovedEventHandler(
        IDomainRepository<ChatGroup, Guid> groups,
        ILogger<ChatGroupMemberRemovedEventHandler> logger,
        ICounterService<ChatGroupMembersCount, Guid> membersCounts)
    {
        _groups = groups;
        _logger = logger;
        _membersCounts = membersCounts;
    }

    public async Task HandleAsync(
        ChatGroupMemberRemovedEvent @event,
        CancellationToken cancellationToken = default)
    {
        var group = await _groups.GetAsync(@event.GroupId, cancellationToken);
        if(group is null) return;

        var membersCount = await _membersCounts.Decrement(group.Id, cancellationToken: cancellationToken);
        _logger.LogInformation("Decremented Membership count for Chat Group with Id '{Id}' to {Count} ",
            @event.GroupId, membersCount?.MembersCount);
    }
}
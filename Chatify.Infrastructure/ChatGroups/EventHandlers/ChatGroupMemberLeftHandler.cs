using Chatify.Application.Common.Contracts;
using Chatify.Domain.Events.Groups;
using Chatify.Infrastructure.Data.Models;
using Chatify.Shared.Abstractions.Events;

namespace Chatify.Infrastructure.ChatGroups.EventHandlers;

internal sealed class ChatGroupMemberLeftHandler : IEventHandler<ChatGroupMemberLeftEvent>
{
    private readonly ICounterService<ChatGroupMembersCount, Guid> _memberCounts;

    public ChatGroupMemberLeftHandler(
        ICounterService<ChatGroupMembersCount, Guid> memberCounts)
        => _memberCounts = memberCounts;

    public async Task HandleAsync(
        ChatGroupMemberLeftEvent @event,
        CancellationToken cancellationToken = default)
    {
        await _memberCounts.Decrement(@event.GroupId, cancellationToken: cancellationToken);
    }
}
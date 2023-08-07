using Chatify.Application.Common.Contracts;
using Chatify.Domain.Events.Groups;
using Chatify.Infrastructure.Data.Models;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.ChatGroups.EventHandlers;

internal sealed class ChatGroupMemberLeftHandler
    : IEventHandler<ChatGroupMemberLeftEvent>
{
    private readonly ICounterService<ChatGroupMembersCount, Guid> _memberCounts;
    private readonly IHubContext<ChatifyHub, IChatifyHubClient> _chatifyHubContext;
    private readonly IIdentityContext _identityContext;

    public ChatGroupMemberLeftHandler(
        ICounterService<ChatGroupMembersCount, Guid> memberCounts,
        IHubContext<ChatifyHub, IChatifyHubClient> chatifyHubContext,
        IIdentityContext identityContext)
    {
        _memberCounts = memberCounts;
        _chatifyHubContext = chatifyHubContext;
        _identityContext = identityContext;
    }
    
    // private static RedisKey GetGroupMembersCacheKey(Guid groupId)
    //     => new($"groups:{groupId.ToString()}:members");

    public async Task HandleAsync(
        ChatGroupMemberLeftEvent @event,
        CancellationToken cancellationToken = default)
    {
        await _memberCounts.Decrement(@event.GroupId, cancellationToken: cancellationToken);
        
        var groupId = $"chat-groups:{@event.GroupId}";
        await _chatifyHubContext
            .Clients
            .Group(groupId)
            .ChatGroupMemberLeft(new ChatGroupMemberLeft(
                @event.GroupId,
                @event.UserId,
                _identityContext.Username,
                @event.Timestamp));
    }
}
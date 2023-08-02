using Chatify.Application.Common.Contracts;
using Chatify.Domain.Events.Messages;
using Chatify.Infrastructure.Data.Models;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.Messages.EventHandlers;

internal sealed class ChatMessageReplyDeletedEventHandler
    : IEventHandler<ChatMessageReplyDeletedEvent>
{
    private readonly ICounterService<ChatMessageReplyCount, Guid> _replyCounts;
    private readonly IHubContext<ChatifyHub, IChatifyHubClient> _chatifyContext;
    private readonly IIdentityContext _identityContext;

    public ChatMessageReplyDeletedEventHandler(
        ICounterService<ChatMessageReplyCount, Guid> replyCounts,
        IIdentityContext identityContext,
        IHubContext<ChatifyHub, IChatifyHubClient> chatifyContext)
    {
        _replyCounts = replyCounts;
        _identityContext = identityContext;
        _chatifyContext = chatifyContext;
    }

    public async Task HandleAsync(ChatMessageReplyDeletedEvent @event, CancellationToken cancellationToken = default)
    {
        await _replyCounts.Decrement(
            @event.ReplyToId,
            cancellationToken: cancellationToken);
        
        var groupId = $"chat-groups:{@event.GroupId}";
        await _chatifyContext
            .Clients
            .Group(groupId)
            .ChatGroupMessageRemoved(
                new ChatGroupMessageRemoved(
                    @event.GroupId,
                    @event.MessageId,
                    @event.UserId,
                    _identityContext.Username,
                    @event.Timestamp));

    }
}
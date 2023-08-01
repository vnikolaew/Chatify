using Chatify.Application.Common.Contracts;
using Chatify.Domain.Events.Messages;
using Chatify.Infrastructure.Data.Models;
using Chatify.Shared.Abstractions.Events;

namespace Chatify.Infrastructure.Messages.EventHandlers;

internal sealed class ChatMessageReplyDeletedEventHandler : IEventHandler<ChatMessageReplyDeletedEvent>
{
    private readonly ICounterService<ChatMessageReplyCount, Guid> _replyCounts;

    public ChatMessageReplyDeletedEventHandler(ICounterService<ChatMessageReplyCount, Guid> replyCounts)
        => _replyCounts = replyCounts;

    public Task HandleAsync(ChatMessageReplyDeletedEvent @event, CancellationToken cancellationToken = default)
        => _replyCounts.Decrement(
            @event.ReplyToId,
            cancellationToken: cancellationToken);
}
using Chatify.Application.Common.Contracts;
using Chatify.Domain.Events.Messages;
using Chatify.Infrastructure.Data.Models;
using Chatify.Shared.Abstractions.Events;

namespace Chatify.Infrastructure.Messages.EventHandlers;

internal sealed class ChatMessageRepliedToEventHandler : IEventHandler<ChatMessageRepliedToEvent>
{
    private readonly ICounterService<ChatMessageReplyCount, Guid> _replyCounts;

    public ChatMessageRepliedToEventHandler(
        ICounterService<ChatMessageReplyCount, Guid> replyCounts)
        => _replyCounts = replyCounts;

    public async Task HandleAsync(
        ChatMessageRepliedToEvent @event,
        CancellationToken cancellationToken = default)
    {
        await _replyCounts.Increment(
            @event.ReplyToId,
            cancellationToken: cancellationToken);
    }
}
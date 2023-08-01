using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Messages;
using Chatify.Shared.Abstractions.Events;
using Microsoft.Extensions.Logging;

namespace Chatify.Application.Messages.EventHandlers;

internal sealed class ChatMessageDeletedEventHandler : IEventHandler<ChatMessageDeletedEvent>
{
    private readonly IDomainRepository<ChatMessage, Guid> _messages;
    private readonly ILogger<ChatMessageDeletedEventHandler> _logger;
    private readonly IChatMessageReplyRepository _messageReplies;

    public ChatMessageDeletedEventHandler(
        IDomainRepository<ChatMessage, Guid> messages,
        IChatMessageReplyRepository  messageReplies,
        ILogger<ChatMessageDeletedEventHandler> logger)
    {
        _messages = messages;
        _messageReplies = messageReplies;
        _logger = logger;
    }

    public async Task HandleAsync(ChatMessageDeletedEvent @event, CancellationToken cancellationToken = default)
    {
        // Delete all replies related to the deleted message as well
        await _messageReplies.DeleteAllForMessage(@event.MessageId, cancellationToken);
        _logger.LogInformation("Deleted all replies for message with Id '{Id}'", @event.MessageId);
    }
}
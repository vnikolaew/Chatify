using Chatify.Application.Messages.Contracts;
using Chatify.Domain.Events.Messages;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Events;
using Microsoft.Extensions.Logging;

namespace Chatify.Application.Messages.EventHandlers;

internal sealed class ChatMessageDeletedEventHandler : IEventHandler<ChatMessageDeletedEvent>
{
    private readonly ILogger<ChatMessageDeletedEventHandler> _logger;
    private readonly IChatMessageReplyRepository _messageReplies;
    private readonly INotificationService _notificationService;

    public ChatMessageDeletedEventHandler(
        IChatMessageReplyRepository  messageReplies,
        ILogger<ChatMessageDeletedEventHandler> logger,
        INotificationService notificationService)
    {
        _messageReplies = messageReplies;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task HandleAsync(ChatMessageDeletedEvent @event, CancellationToken cancellationToken = default)
    {
        // Delete all replies related to the deleted message as well
        await _messageReplies.DeleteAllForMessage(@event.MessageId, cancellationToken);
        _logger.LogInformation("Deleted all replies for message with Id '{Id}'", @event.MessageId);
        
        await _notificationService.NotifyChatMessageDeleted(@event, cancellationToken);
    }
}
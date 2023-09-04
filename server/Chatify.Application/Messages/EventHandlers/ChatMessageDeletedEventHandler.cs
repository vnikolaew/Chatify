using Chatify.Application.Messages.Contracts;
using Chatify.Domain.Events.Messages;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Events;
using Microsoft.Extensions.Logging;

namespace Chatify.Application.Messages.EventHandlers;

internal sealed class ChatMessageDeletedEventHandler(IChatMessageReplyRepository messageReplies,
        ILogger<ChatMessageDeletedEventHandler> logger,
        INotificationService notificationService,
        IChatMessageReplierInfosRepository replierInfos)
    : IEventHandler<ChatMessageDeletedEvent>
{
    public async Task HandleAsync(ChatMessageDeletedEvent @event, CancellationToken cancellationToken = default)
    {
        // Delete all replies related to the deleted message as well
        await messageReplies.DeleteAllForMessage(@event.MessageId, cancellationToken);
        
        // Delete all replier infoes related to the deleted message as well
        await replierInfos.DeleteAllForMessage(@event.MessageId, cancellationToken);
        
        logger.LogInformation("Deleted all replies for message with Id '{Id}'", @event.MessageId);
        await notificationService.NotifyChatMessageDeleted(@event, cancellationToken);
    }
}
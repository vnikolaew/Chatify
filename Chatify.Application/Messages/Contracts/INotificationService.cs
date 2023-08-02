using Chatify.Domain.Events.Messages;

namespace Chatify.Application.Messages.Contracts;

public interface INotificationService
{
    Task NotifyChatMessageDeleted(
        ChatMessageDeletedEvent @event,
        CancellationToken cancellationToken = default);
}
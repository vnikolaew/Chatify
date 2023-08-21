using Chatify.Application.Messages.Contracts;
using Chatify.Domain.Events.Messages;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Contexts;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.Messages;

internal sealed class SignalRNotificationService(IHubContext<ChatifyHub, IChatifyHubClient> chatifyHubContext,
        IIdentityContext identityContext)
    : INotificationService
{
    public async Task NotifyChatMessageDeleted(
        ChatMessageDeletedEvent @event,
        CancellationToken cancellationToken = default)
    {
        await chatifyHubContext
            .Clients
            .Group(ChatifyHub.GetChatGroupId(@event.GroupId))
            .ChatGroupMessageRemoved(new ChatGroupMessageRemoved(
                @event.GroupId,
                @event.MessageId,
                @event.UserId,
                identityContext.Username,
                @event.Timestamp));

    }
}
using Chatify.Application.Messages.Contracts;
using Chatify.Domain.Events.Messages;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Contexts;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.Messages;

internal sealed class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<ChatifyHub, IChatifyHubClient> _chatifyHubContext;
    private readonly IIdentityContext _identityContext;

    public SignalRNotificationService(
        IHubContext<ChatifyHub, IChatifyHubClient> chatifyHubContext,
        IIdentityContext identityContext)
    {
        _chatifyHubContext = chatifyHubContext;
        _identityContext = identityContext;
    }

    public async Task NotifyChatMessageDeleted(
        ChatMessageDeletedEvent @event,
        CancellationToken cancellationToken = default)
    {
        var groupId = $"chat-groups:{@event.GroupId}";
        await _chatifyHubContext
            .Clients
            .Group(groupId)
            .ChatGroupMessageRemoved(new ChatGroupMessageRemoved(
                @event.GroupId,
                @event.MessageId,
                @event.UserId,
                _identityContext.Username,
                @event.Timestamp));

    }
}
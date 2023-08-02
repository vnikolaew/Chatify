using Chatify.Domain.Events.Messages;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Client;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.Messages.EventHandlers;

internal sealed class ChatMessageSentEventHandler : IEventHandler<ChatMessageSentEvent>
{
    private readonly IHubContext<ChatifyHub, IChatifyHubClient> _chatifyHubContext;
    private readonly IIdentityContext _identityContext;

    public ChatMessageSentEventHandler(
        IHubContext<ChatifyHub, IChatifyHubClient> chatifyHubContext,
        IIdentityContext identityContext)
    {
        _chatifyHubContext = chatifyHubContext;
        _identityContext = identityContext;
    }

    public async Task HandleAsync(
        ChatMessageSentEvent @event,
        CancellationToken cancellationToken = default)
    {
        var groupId = $"chat-groups:{@event.GroupId}";
        await _chatifyHubContext
            .Clients
            .Group(groupId)
            .ReceiveGroupChatMessage(
                new ReceiveGroupChatMessage(
                    @event.GroupId,
                    @event.UserId,
                    @event.MessageId,
                    _identityContext.Username,
                    @event.Content,
                    @event.Timestamp));
    }
}
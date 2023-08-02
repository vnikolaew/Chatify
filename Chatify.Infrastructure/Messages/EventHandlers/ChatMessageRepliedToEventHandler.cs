using Chatify.Application.Common.Contracts;
using Chatify.Domain.Events.Messages;
using Chatify.Infrastructure.Data.Models;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.Messages.EventHandlers;

internal sealed class ChatMessageRepliedToEventHandler
    : IEventHandler<ChatMessageRepliedToEvent>
{
    private readonly ICounterService<ChatMessageReplyCount, Guid> _replyCounts;
    private readonly IHubContext<ChatifyHub, IChatifyHubClient> _chatifyContext;
    private readonly IIdentityContext _identityContext;

    public ChatMessageRepliedToEventHandler(
        ICounterService<ChatMessageReplyCount, Guid> replyCounts,
        IHubContext<ChatifyHub, IChatifyHubClient> chatifyContext,
        IIdentityContext identityContext)
    {
        _replyCounts = replyCounts;
        _chatifyContext = chatifyContext;
        _identityContext = identityContext;
    }

    public async Task HandleAsync(
        ChatMessageRepliedToEvent @event,
        CancellationToken cancellationToken = default)
    {
        await _replyCounts.Increment(
            @event.ReplyToId,
            cancellationToken: cancellationToken);

        var groupId = $"chat-groups:{@event.GroupId}";
        await _chatifyContext
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
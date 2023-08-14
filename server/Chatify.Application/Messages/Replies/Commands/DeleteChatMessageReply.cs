using System.ComponentModel.DataAnnotations;
using Chatify.Application.Messages.Common;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Messages;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using OneOf;

namespace Chatify.Application.Messages.Replies.Commands;

using DeleteChatMessageReplyResult = OneOf<MessageNotFoundError, UserIsNotMessageSenderError, Unit>;

public record DeleteChatMessageReply(
    [Required] Guid ReplyMessageId,
    [Required] Guid GroupId
) : ICommand<DeleteChatMessageReplyResult>;

internal sealed class DeleteChatMessageReplyHandler
    : ICommandHandler<DeleteChatMessageReply, DeleteChatMessageReplyResult>
{
    private readonly IIdentityContext _identityContext;
    private readonly IDomainRepository<ChatMessageReply, Guid> _messageReplies;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IClock _clock;

    public DeleteChatMessageReplyHandler(
        IIdentityContext identityContext,
        IDomainRepository<ChatMessageReply, Guid> messageReplies,
        IEventDispatcher eventDispatcher,
        IClock clock)
    {
        _identityContext = identityContext;
        _messageReplies = messageReplies;
        _eventDispatcher = eventDispatcher;
        _clock = clock;
    }

    public async Task<DeleteChatMessageReplyResult> HandleAsync(
        DeleteChatMessageReply command,
        CancellationToken cancellationToken = default)
    {
        var replyMessage = await _messageReplies.GetAsync(command.ReplyMessageId, cancellationToken);
        if ( replyMessage is null ) return new MessageNotFoundError(command.ReplyMessageId);

        if ( replyMessage.UserId != _identityContext.Id )
            return new UserIsNotMessageSenderError(replyMessage.Id, _identityContext.Id);

        var success = await _messageReplies.DeleteAsync(replyMessage.Id, cancellationToken);
        await _eventDispatcher.PublishAsync(new ChatMessageReplyDeletedEvent
        {
            MessageId = replyMessage.ReplyToId,
            GroupId = replyMessage.ChatGroupId,
            UserId = replyMessage.UserId,
            ReplyToId = replyMessage.ReplyToId,
            Timestamp = _clock.Now
        }, cancellationToken);

        return Unit.Default;
    }
}
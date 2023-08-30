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

internal sealed class DeleteChatMessageReplyHandler(IIdentityContext identityContext,
        IDomainRepository<ChatMessageReply, Guid> messageReplies,
        IEventDispatcher eventDispatcher,
        IClock clock)
    : ICommandHandler<DeleteChatMessageReply, DeleteChatMessageReplyResult>
{
    public async Task<DeleteChatMessageReplyResult> HandleAsync(
        DeleteChatMessageReply command,
        CancellationToken cancellationToken = default)
    {
        var replyMessage = await messageReplies.GetAsync(command.ReplyMessageId, cancellationToken);
        if ( replyMessage is null ) return new MessageNotFoundError(command.ReplyMessageId);

        if ( replyMessage.UserId != identityContext.Id )
            return new UserIsNotMessageSenderError(replyMessage.Id, identityContext.Id);

        var success = await messageReplies.DeleteAsync(replyMessage.Id, cancellationToken);
        await eventDispatcher.PublishAsync(new ChatMessageReplyDeletedEvent
        {
            MessageId = replyMessage.ReplyToId,
            GroupId = replyMessage.ChatGroupId,
            UserId = replyMessage.UserId,
            ReplyToId = replyMessage.ReplyToId,
            Timestamp = clock.Now
        }, cancellationToken);

        return Unit.Default;
    }
}
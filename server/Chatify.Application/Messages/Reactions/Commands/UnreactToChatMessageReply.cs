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

namespace Chatify.Application.Messages.Reactions.Commands;

using UnreactToChatMessageReplyResult = OneOf.OneOf<
    MessageNotFoundError,
    MessageReactionNotFoundError,
    UserHasNotReactedError,
    Unit>;

public record UnreactToChatMessageReply(
    [Required] Guid MessageReactionId,
    [Required] Guid MessageId,
    [Required] Guid GroupId
) : ICommand<UnreactToChatMessageReplyResult>;

internal sealed class UnreactToChatMessageReplyHandler(IIdentityContext identityContext,
        IDomainRepository<ChatMessageReply, Guid> messageReplies,
        IDomainRepository<ChatMessageReaction, Guid> messageReactions,
        IEventDispatcher eventDispatcher,
        IClock clock)
    : ICommandHandler<UnreactToChatMessageReply, UnreactToChatMessageReplyResult>
{
    public async Task<UnreactToChatMessageReplyResult> HandleAsync(
        UnreactToChatMessageReply command,
        CancellationToken cancellationToken = default)
    {
        var replyMessage = await messageReplies.GetAsync(command.MessageId, cancellationToken);
        if ( replyMessage is null ) return new MessageNotFoundError(command.MessageId);

        var messageReaction = await messageReactions.GetAsync(command.MessageReactionId, cancellationToken);

        if ( messageReaction is null ) return new MessageReactionNotFoundError();
        if ( messageReaction.UserId != identityContext.Id ) return new UserHasNotReactedError();

        await messageReactions.DeleteAsync(messageReaction.Id, cancellationToken);
        await messageReplies.UpdateAsync(replyMessage.Id, message =>
        {
            message.UpdatedAt = clock.Now;
            message.DecrementReactionCount(messageReaction.ReactionCode);
        }, cancellationToken);

        await eventDispatcher.PublishAsync(
            new ChatMessageUnreactedToEvent
            {
                MessageId = replyMessage.Id,
                MessageReactionId = messageReaction.Id,
                GroupId = replyMessage.ChatGroupId,
                UserId = messageReaction.UserId,
                ReactionCode = messageReaction.ReactionCode,
                Timestamp = clock.Now
            }, cancellationToken);

        return Unit.Default;
    }
}
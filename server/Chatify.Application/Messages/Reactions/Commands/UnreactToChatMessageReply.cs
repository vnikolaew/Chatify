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

using UnreactToChatMessageReplyResult  = OneOf.OneOf<
    MessageNotFoundError,
    MessageReactionNotFoundError,
    UserHasNotReactedError,
    Unit>;

public record UnreactToChatMessageReply(
    [Required] Guid MessageReactionId,
    [Required] Guid MessageId,
    [Required] Guid GroupId
) : ICommand<UnreactToChatMessageReplyResult>;

internal sealed class UnreactToChatMessageReplyHandler
    : ICommandHandler<UnreactToChatMessageReply, UnreactToChatMessageReplyResult>
{
    private readonly IIdentityContext _identityContext;
    private readonly IDomainRepository<ChatMessageReply, Guid> _messageReplies;
    private readonly IDomainRepository<ChatMessageReaction, Guid> _messageReactions;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IClock _clock;

    public UnreactToChatMessageReplyHandler(
        IIdentityContext identityContext,
        IDomainRepository<ChatMessageReply, Guid> messageReplies,
        IDomainRepository<ChatMessageReaction, Guid> messageReactions,
        IEventDispatcher eventDispatcher,
        IClock clock)
    {
        _identityContext = identityContext;
        _messageReplies = messageReplies;
        _messageReactions = messageReactions;
        _eventDispatcher = eventDispatcher;
        _clock = clock;
    }

    public async Task<UnreactToChatMessageReplyResult> HandleAsync(
        UnreactToChatMessageReply command,
        CancellationToken cancellationToken = default)
    {
        var replyMessage = await _messageReplies.GetAsync(command.MessageId, cancellationToken);
        if ( replyMessage is null ) return new MessageNotFoundError(command.MessageId);
            
        var messageReaction = await _messageReactions.GetAsync(command.MessageReactionId, cancellationToken);

        if ( messageReaction is null ) return new MessageReactionNotFoundError();
        if ( messageReaction.UserId != _identityContext.Id ) return new UserHasNotReactedError();

        await _messageReactions.DeleteAsync(messageReaction.Id, cancellationToken);
        await _messageReplies.UpdateAsync(replyMessage.Id, message =>
        {
            message.UpdatedAt = _clock.Now;
            message.DecrementReactionCount(messageReaction.ReactionType);
        }, cancellationToken);

        await _eventDispatcher.PublishAsync(new ChatMessageUnreactedToEvent
        {
            MessageId = replyMessage.Id,
            MessageReactionId = messageReaction.Id,
            GroupId = replyMessage.ChatGroupId,
            UserId = messageReaction.UserId,
            ReactionType = messageReaction.ReactionType,
            Timestamp = _clock.Now
        }, cancellationToken);
        
        return Unit.Default;
    }
}
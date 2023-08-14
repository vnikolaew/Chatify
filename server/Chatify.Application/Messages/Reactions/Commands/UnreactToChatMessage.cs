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

namespace Chatify.Application.Messages.Reactions.Commands;

using UnreactToChatMessageResult = OneOf<
    MessageNotFoundError,
    MessageReactionNotFoundError,
    UserHasNotReactedError,
    Unit>;

public record MessageReactionNotFoundError;

public record UserHasNotReactedError;

public record UnreactToChatMessage(
    [Required] Guid MessageReactionId,
    [Required] Guid MessageId,
    [Required] Guid GroupId) : ICommand<UnreactToChatMessageResult>;

internal sealed class UnreactToChatMessageHandler : ICommandHandler<UnreactToChatMessage, UnreactToChatMessageResult>
{
    private readonly IIdentityContext _identityContext;
    private readonly IDomainRepository<ChatMessage, Guid> _messages;
    private readonly IDomainRepository<ChatMessageReaction, Guid> _messageReactions;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IClock _clock;

    public UnreactToChatMessageHandler(
        IIdentityContext identityContext,
        IDomainRepository<ChatMessage, Guid> messages,
        IDomainRepository<ChatMessageReaction, Guid> messageReactions,
        IEventDispatcher eventDispatcher,
        IClock clock)
    {
        _identityContext = identityContext;
        _messages = messages;
        _messageReactions = messageReactions;
        _eventDispatcher = eventDispatcher;
        _clock = clock;
    }

    public async Task<UnreactToChatMessageResult> HandleAsync(
        UnreactToChatMessage command,
        CancellationToken cancellationToken = default)
    {
        var message = await _messages.GetAsync(command.MessageId, cancellationToken);
        if ( message is null ) return new MessageNotFoundError(command.MessageId);

        var messageReaction = await _messageReactions.GetAsync(command.MessageReactionId, cancellationToken);
        if ( messageReaction is null ) return new MessageReactionNotFoundError();
        if ( messageReaction.UserId != _identityContext.Id ) return new UserHasNotReactedError();

        await _messageReactions.DeleteAsync(messageReaction.Id, cancellationToken);
        await _messages.UpdateAsync(message.Id, message =>
        {
            message.UpdatedAt = _clock.Now;
            message.DecrementReactionCount(messageReaction.ReactionType);
        }, cancellationToken);

        await _eventDispatcher.PublishAsync(new ChatMessageUnreactedToEvent
        {
            MessageId = message.Id,
            MessageReactionId = messageReaction.Id,
            GroupId = message.ChatGroupId,
            UserId = messageReaction.UserId,
            ReactionType = messageReaction.ReactionType,
            Timestamp = _clock.Now
        }, cancellationToken);

        return Unit.Default;
    }
}
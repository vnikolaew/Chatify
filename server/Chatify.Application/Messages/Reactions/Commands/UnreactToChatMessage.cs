using System.ComponentModel.DataAnnotations;
using Chatify.Application.Messages.Common;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Messages;
using Chatify.Domain.Repositories;
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

internal sealed class UnreactToChatMessageHandler(IIdentityContext identityContext,
        IChatMessageRepository messages,
        IChatMessageReactionRepository messageReactions,
        IEventDispatcher eventDispatcher,
        IClock clock)
    : ICommandHandler<UnreactToChatMessage, UnreactToChatMessageResult>
{
    public async Task<UnreactToChatMessageResult> HandleAsync(
        UnreactToChatMessage command,
        CancellationToken cancellationToken = default)
    {
        var message = await messages.GetAsync(command.MessageId, cancellationToken);
        if ( message is null ) return new MessageNotFoundError(command.MessageId);

        var messageReaction = await messageReactions.GetAsync(command.MessageReactionId, cancellationToken);
        if ( messageReaction is null ) return new MessageReactionNotFoundError();
        if ( messageReaction.UserId != identityContext.Id ) return new UserHasNotReactedError();

        await messageReactions.DeleteAsync(messageReaction, cancellationToken);
        await messages.UpdateAsync(message.Id, message =>
        {
            message.UpdatedAt = clock.Now;
            message.DecrementReactionCount(messageReaction.ReactionCode);
        }, cancellationToken);

        await eventDispatcher.PublishAsync(
            new ChatMessageUnreactedToEvent
            {
                MessageId = message.Id,
                MessageReactionId = messageReaction.Id,
                GroupId = message.ChatGroupId,
                UserId = messageReaction.UserId,
                ReactionCode = messageReaction.ReactionCode,
                Timestamp = clock.Now
            }, cancellationToken);

        return Unit.Default;
    }
}
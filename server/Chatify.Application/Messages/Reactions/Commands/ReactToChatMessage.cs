using System.ComponentModel.DataAnnotations;
using Chatify.Application.Messages.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Messages;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Common;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.Messages.Reactions.Commands;

using ReactToChatMessageResult = OneOf<Error, MessageNotFoundError, UserIsNotMemberError, Guid>;

public record ReactToChatMessage(
    [Required] Guid MessageId,
    [Required] Guid GroupId,
    [Required] long ReactionCode
) : ICommand<ReactToChatMessageResult>;

internal sealed class ReactToChatMessageHandler(IChatGroupMemberRepository members,
        IIdentityContext identityContext,
        IChatMessageRepository messages,
        IEventDispatcher eventDispatcher, IClock clock,
        IGuidGenerator guidGenerator,
        IChatMessageReactionRepository messageReactions)
    : ICommandHandler<ReactToChatMessage, ReactToChatMessageResult>
{
    public async Task<ReactToChatMessageResult> HandleAsync(
        ReactToChatMessage command,
        CancellationToken cancellationToken = default)
    {
        var userIsGroupMember = await members.Exists(
            command.GroupId,
            identityContext.Id, cancellationToken);
        if ( !userIsGroupMember ) return new UserIsNotMemberError(identityContext.Id, command.GroupId);

        var message = await messages.GetAsync(command.MessageId, cancellationToken);
        if ( message is null ) return new MessageNotFoundError(command.MessageId);

        var existingReaction = await messageReactions
            .ByMessageAndUser(
                command.MessageId,
                identityContext.Id,
                cancellationToken);
        if (existingReaction is not null)
        {
            var oldReactionType = existingReaction.ReactionCode;
            if ( oldReactionType == command.ReactionCode )
            {
                return Error.New("User has already reacted with the same code.");
            }
            
            await messageReactions.UpdateAsync(existingReaction.Id, reaction =>
            {
                reaction.ReactionCode = command.ReactionCode;
                reaction.UpdatedAt = clock.Now;
            }, cancellationToken);
            
            await messages.UpdateAsync(message.Id, message =>
            {
                message.UpdatedAt = clock.Now;
                message.ChangeReaction(oldReactionType, command.ReactionCode);
            }, cancellationToken);
            return existingReaction.Id;
        }

        var messageReactionId = guidGenerator.New();
        var messageReaction = new ChatMessageReaction
        {
            Id = messageReactionId,
            CreatedAt = clock.Now,
            ReactionCode = command.ReactionCode,
            Message = message,
            UserId = identityContext.Id,
            Username = identityContext.Username,
            ChatGroupId = command.GroupId
        };

        await messageReactions.SaveAsync(messageReaction, cancellationToken);
        await messages.UpdateAsync(message.Id, message =>
        {
            message.UpdatedAt = clock.Now;
            message.IncrementReactionCount(command.ReactionCode);
        }, cancellationToken);
        await eventDispatcher.PublishAsync(new ChatMessageReactedToEvent
        {
            MessageId = message.Id,
            MessageReactionId = messageReaction.Id,
            GroupId = messageReaction.ChatGroupId,
            UserId = messageReaction.UserId,
            ReactionCode = messageReaction.ReactionCode,
            Timestamp = clock.Now
        }, cancellationToken);

        return messageReaction.Id;
    }
}
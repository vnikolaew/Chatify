using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Contracts;
using Chatify.Application.Messages.Common;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Messages;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Common;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt.Common;

namespace Chatify.Application.Messages.Reactions.Commands;

using ReactToChatMessageReplyResult = OneOf.OneOf<Error, MessageNotFoundError, UserIsNotMemberError, Guid>;

public record ReactToChatMessageReply(
    [Required] Guid MessageId,
    [Required] Guid GroupId,
    [Required] long ReactionType
) : ICommand<ReactToChatMessageReplyResult>;

internal sealed class ReactToChatMessageReplyHandler(IChatGroupMemberRepository members,
        IIdentityContext identityContext,
        IDomainRepository<ChatMessageReply, Guid> messageReplies,
        IChatMessageReactionRepository messageReactions,
        IEventDispatcher eventDispatcher,
        IClock clock,
        IGuidGenerator guidGenerator)
    : ICommandHandler<ReactToChatMessageReply, ReactToChatMessageReplyResult>
{
    public async Task<ReactToChatMessageReplyResult> HandleAsync(
        ReactToChatMessageReply command,
        CancellationToken cancellationToken = default)
    {
        var userIsGroupMember = await members.Exists(
            command.GroupId,
            identityContext.Id, cancellationToken);
        if ( !userIsGroupMember ) return new UserIsNotMemberError(identityContext.Id, command.GroupId);

        var replyMessage = await messageReplies.GetAsync(command.MessageId, cancellationToken);
        if ( replyMessage is null ) return new MessageNotFoundError(command.MessageId);

        var existingReaction = await messageReactions
            .ByMessageAndUser(
                command.MessageId,
                identityContext.Id,
                cancellationToken);
        if ( existingReaction is not null )
        {
            var oldReactionType = existingReaction.ReactionCode;
            if ( oldReactionType == command.ReactionType )
            {
                return Error.New("User has already reacted with the same code.");
            }
            
            await messageReactions.UpdateAsync(existingReaction, reaction =>
            {
                reaction.ReactionCode = command.ReactionType;
                reaction.UpdatedAt = clock.Now;
            }, cancellationToken);

            await messageReplies.UpdateAsync(replyMessage, message =>
            {
                message.UpdatedAt = clock.Now;
                message.ChangeReaction(oldReactionType, command.ReactionType);
            }, cancellationToken);
            return existingReaction.Id;
        }

        var messageReactionId = guidGenerator.New();
        var messageReaction = new ChatMessageReaction
        {
            Id = messageReactionId,
            CreatedAt = clock.Now,
            ReactionCode = command.ReactionType,
            Message = replyMessage,
            UserId = identityContext.Id,
            Username = identityContext.Username,
            ChatGroupId = command.GroupId
        };

        await messageReactions.SaveAsync(messageReaction, cancellationToken);
        await messageReplies.UpdateAsync(replyMessage, message =>
        {
            message.UpdatedAt = clock.Now;
            message.IncrementReactionCount(command.ReactionType);
        }, cancellationToken);

        await eventDispatcher.PublishAsync(
            new ChatMessageReactedToEvent
            {
                MessageId = replyMessage.Id,
                MessageReactionId = messageReaction.Id,
                GroupId = messageReaction.ChatGroupId,
                UserId = messageReaction.UserId,
                ReactionCode = messageReaction.ReactionCode,
                Timestamp = clock.Now
            }, cancellationToken);

        return messageReaction.Id;
    }
}
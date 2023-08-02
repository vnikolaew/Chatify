using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Contracts;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Messages;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using LanguageExt.Common;

namespace Chatify.Application.Messages.Reactions.Commands;

using ReactToChatMessageReplyResult = Either<Error, Guid>;

public record ReactToChatMessageReply(
    [Required] Guid MessageId,
    [Required] Guid GroupId,
    [Required] sbyte ReactionType
) : ICommand<ReactToChatMessageReplyResult>;

internal sealed class ReactToChatMessageReplyHandler
    : ICommandHandler<ReactToChatMessageReply, ReactToChatMessageReplyResult>
{
    private readonly IChatGroupMemberRepository _members;
    private readonly IIdentityContext _identityContext;
    private readonly IDomainRepository<ChatMessageReply, Guid> _messageReplies;
    private readonly IChatMessageReactionRepository _messageReactions;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IClock _clock;
    private readonly IGuidGenerator _guidGenerator;

    public ReactToChatMessageReplyHandler(
        IChatGroupMemberRepository members,
        IIdentityContext identityContext,
        IDomainRepository<ChatMessageReply, Guid> messageReplies,
        IChatMessageReactionRepository messageReactions,
        IEventDispatcher eventDispatcher,
        IClock clock,
        IGuidGenerator guidGenerator)
    {
        _members = members;
        _identityContext = identityContext;
        _messageReplies = messageReplies;
        _messageReactions = messageReactions;
        _eventDispatcher = eventDispatcher;
        _clock = clock;
        _guidGenerator = guidGenerator;
    }

    public async Task<ReactToChatMessageReplyResult> HandleAsync(
        ReactToChatMessageReply command,
        CancellationToken cancellationToken = default)
    {
        var userIsGroupMember = await _members.Exists(
            command.GroupId,
            _identityContext.Id, cancellationToken);
        if (!userIsGroupMember) return Error.New("");

        var replyMessage = await _messageReplies.GetAsync(command.MessageId, cancellationToken);
        if (replyMessage is null) return Error.New("");

        var existingReaction = await _messageReactions
            .ByMessageAndUser(
                command.MessageId,
                _identityContext.Id,
                cancellationToken);
        if (existingReaction is not null)
        {
            var oldReactionType = existingReaction.ReactionType;
            await _messageReactions.UpdateAsync(existingReaction.Id, reaction =>
            {
                reaction.ReactionType = command.ReactionType;
                reaction.UpdatedAt = _clock.Now;
            }, cancellationToken);
            
            await _messageReplies.UpdateAsync(replyMessage.Id, message =>
            {
                message.UpdatedAt = _clock.Now;
                message.ChangeReaction(oldReactionType, command.ReactionType);
            }, cancellationToken);
            return existingReaction.Id;
        }

        var messageReactionId = _guidGenerator.New();
        var messageReaction = new ChatMessageReaction
        {
            Id = messageReactionId,
            CreatedAt = _clock.Now,
            ReactionType = command.ReactionType,
            Message = replyMessage,
            UserId = _identityContext.Id,
            ChatGroupId = command.GroupId
        };

        await _messageReactions.SaveAsync(messageReaction, cancellationToken);
        await _messageReplies.UpdateAsync(replyMessage.Id, message =>
        {
            message.UpdatedAt = _clock.Now;
            message.IncrementReactionCount(command.ReactionType);
        }, cancellationToken);

        await _eventDispatcher.PublishAsync(new ChatMessageReactedToEvent
        {
            MessageId = replyMessage.Id,
            MessageReactionId = messageReaction.Id,
            GroupId = messageReaction.ChatGroupId,
            UserId = messageReaction.UserId,
            ReactionType = messageReaction.ReactionType,
            Timestamp = _clock.Now
        }, cancellationToken);

        return messageReaction.Id;
    }
}
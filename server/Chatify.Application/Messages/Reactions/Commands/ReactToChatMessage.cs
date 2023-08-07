using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.Common.Contracts;
using Chatify.Application.Messages.Replies.Queries;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Messages;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using OneOf;

namespace Chatify.Application.Messages.Reactions.Commands;

using ReactToChatMessageResult = OneOf<MessageNotFoundError, UserIsNotMemberError, Guid>;

public record ReactToChatMessage(
    [Required] Guid MessageId,
    [Required] Guid GroupId,
    [Required] sbyte ReactionType
) : ICommand<ReactToChatMessageResult>;

internal sealed class ReactToChatMessageHandler
    : ICommandHandler<ReactToChatMessage, ReactToChatMessageResult>
{
    private readonly IChatGroupMemberRepository _members;
    private readonly IIdentityContext _identityContext;
    private readonly IDomainRepository<ChatMessage, Guid> _messages;
    private readonly IChatMessageReactionRepository _messageReactions;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IClock _clock;
    private readonly IGuidGenerator _guidGenerator;

    public ReactToChatMessageHandler(
        IChatGroupMemberRepository members,
        IIdentityContext identityContext,
        IDomainRepository<ChatMessage, Guid> messages, IEventDispatcher eventDispatcher, IClock clock,
        IGuidGenerator guidGenerator,
        IChatMessageReactionRepository messageReactions)
    {
        _members = members;
        _identityContext = identityContext;
        _messages = messages;
        _eventDispatcher = eventDispatcher;
        _clock = clock;
        _guidGenerator = guidGenerator;
        _messageReactions = messageReactions;
    }

    public async Task<ReactToChatMessageResult> HandleAsync(
        ReactToChatMessage command,
        CancellationToken cancellationToken = default)
    {
        var userIsGroupMember = await _members.Exists(
            command.GroupId,
            _identityContext.Id, cancellationToken);
        if ( !userIsGroupMember ) return new UserIsNotMemberError(_identityContext.Id, command.GroupId);

        var message = await _messages.GetAsync(command.MessageId, cancellationToken);
        if ( message is null ) return new MessageNotFoundError(command.MessageId);

        var existingReaction = await _messageReactions
            .ByMessageAndUser(
                command.MessageId,
                _identityContext.Id,
                cancellationToken);
        if (existingReaction is not null &&
            existingReaction.ReactionType != command.ReactionType)
        {
            var oldReactionType = existingReaction.ReactionType;
            await _messageReactions.UpdateAsync(existingReaction.Id, reaction =>
            {
                reaction.ReactionType = command.ReactionType;
                reaction.UpdatedAt = _clock.Now;
            }, cancellationToken);
            
            await _messages.UpdateAsync(message.Id, message =>
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
            Message = message,
            UserId = _identityContext.Id,
            Username = _identityContext.Username,
            ChatGroupId = command.GroupId
        };

        await _messageReactions.SaveAsync(messageReaction, cancellationToken);
        await _messages.UpdateAsync(message.Id, message =>
        {
            message.UpdatedAt = _clock.Now;
            message.IncrementReactionCount(command.ReactionType);
        }, cancellationToken);
        await _eventDispatcher.PublishAsync(new ChatMessageReactedToEvent
        {
            MessageId = message.Id,
            MessageReactionId = messageReaction.Id,
            GroupId = messageReaction.ChatGroupId,
            UserId = messageReaction.UserId,
            ReactionType = messageReaction.ReactionType,
            Timestamp = _clock.Now
        }, cancellationToken);

        return messageReaction.Id;
    }
}
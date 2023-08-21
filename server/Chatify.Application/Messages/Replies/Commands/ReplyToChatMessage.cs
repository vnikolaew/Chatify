using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Contracts;
using Chatify.Application.Common.Models;
using Chatify.Application.Messages.Common;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Messages;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using OneOf;

namespace Chatify.Application.Messages.Replies.Commands;

using ReplyToChatMessageResult = OneOf<UserIsNotMemberError, MessageNotFoundError, Guid>;

public record ReplyToChatMessage(
    [Required] Guid GroupId,
    [Required] Guid ReplyToId,
    [Required, MinLength(1), MaxLength(500)]
    string Content,
    [Required] IEnumerable<InputFile>? Attachments
) : ICommand<ReplyToChatMessageResult>;

internal sealed class ReplyToChatMessageHandler
    : ICommandHandler<ReplyToChatMessage, ReplyToChatMessageResult>
{
    private readonly IChatGroupMemberRepository _members;
    private readonly IIdentityContext _identityContext;
    private readonly IDomainRepository<ChatMessage, Guid> _messages;
    private readonly IDomainRepository<ChatMessageReply, Guid> _replies;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IClock _clock;
    private readonly IGuidGenerator _guidGenerator;

    public ReplyToChatMessageHandler(
        IChatGroupMemberRepository members,
        IIdentityContext identityContext,
        IDomainRepository<ChatMessage, Guid> messages,
        IEventDispatcher eventDispatcher,
        IClock clock,
        IGuidGenerator guidGenerator,
        IDomainRepository<ChatMessageReply, Guid> replies)
    {
        _members = members;
        _identityContext = identityContext;
        _messages = messages;
        _eventDispatcher = eventDispatcher;
        _clock = clock;
        _guidGenerator = guidGenerator;
        _replies = replies;
    }

    public async Task<ReplyToChatMessageResult> HandleAsync(
        ReplyToChatMessage command,
        CancellationToken cancellationToken = default)
    {
        var userIsGroupMember = await _members.Exists(
            command.GroupId,
            _identityContext.Id, cancellationToken);
        if ( !userIsGroupMember ) return new UserIsNotMemberError(_identityContext.Id, command.GroupId);

        var message = await _messages.GetAsync(command.ReplyToId, cancellationToken);
        if ( message is null ) return new MessageNotFoundError(command.ReplyToId);

        var messageReplyId = _guidGenerator.New();
        var messageReply = new ChatMessageReply
        {
            Id = messageReplyId,
            ChatGroupId = command.GroupId,
            UserId = _identityContext.Id,
            ReplyToId = message.Id,
            Content = command.Content,
            CreatedAt = _clock.Now,
            ReactionCounts = new Dictionary<short, long>()
        };

        await _replies.SaveAsync(messageReply, cancellationToken);
        await _eventDispatcher.PublishAsync(new ChatMessageRepliedToEvent
        {
            UserId = messageReply.UserId,
            Content = messageReply.Content,
            GroupId = messageReply.ChatGroupId,
            Timestamp = _clock.Now,
            ReplyToId = messageReply.ReplyToId,
            MessageId = messageReply.Id
        }, cancellationToken);
        
        return messageReply.Id;
    }
}
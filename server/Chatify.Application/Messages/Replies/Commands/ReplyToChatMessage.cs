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

internal sealed class ReplyToChatMessageHandler(IChatGroupMemberRepository members,
        IIdentityContext identityContext,
        IDomainRepository<ChatMessage, Guid> messages,
        IEventDispatcher eventDispatcher,
        IClock clock,
        IGuidGenerator guidGenerator,
        IDomainRepository<ChatMessageReply, Guid> replies)
    : ICommandHandler<ReplyToChatMessage, ReplyToChatMessageResult>
{
    public async Task<ReplyToChatMessageResult> HandleAsync(
        ReplyToChatMessage command,
        CancellationToken cancellationToken = default)
    {
        var userIsGroupMember = await members.Exists(
            command.GroupId,
            identityContext.Id, cancellationToken);
        if ( !userIsGroupMember ) return new UserIsNotMemberError(identityContext.Id, command.GroupId);

        var message = await messages.GetAsync(command.ReplyToId, cancellationToken);
        if ( message is null ) return new MessageNotFoundError(command.ReplyToId);

        var messageReplyId = guidGenerator.New();
        var messageReply = new ChatMessageReply
        {
            Id = messageReplyId,
            ChatGroupId = command.GroupId,
            UserId = identityContext.Id,
            ReplyToId = message.Id,
            Content = command.Content,
            CreatedAt = clock.Now,
            ReactionCounts = new Dictionary<long, long>()
        };

        await replies.SaveAsync(messageReply, cancellationToken);
        await eventDispatcher.PublishAsync(new ChatMessageRepliedToEvent
        {
            UserId = messageReply.UserId,
            Content = messageReply.Content,
            GroupId = messageReply.ChatGroupId,
            Timestamp = clock.Now,
            ReplyToId = messageReply.ReplyToId,
            MessageId = messageReply.Id
        }, cancellationToken);
        
        return messageReply.Id;
    }
}
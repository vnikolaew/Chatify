using System.ComponentModel.DataAnnotations;
using Chatify.Application.Messages.Commands;
using Chatify.Application.Messages.Common;
using Chatify.Application.Messages.Replies.Queries;
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

namespace Chatify.Application.Messages.Replies.Commands;

using EditChatMessageReplyResult = OneOf<MessageNotFoundError, UserIsNotMessageSenderError, Unit>;

public record EditChatMessageReply(
        [Required] Guid GroupId,
        [Required] Guid MessageId,
        [Required] string NewContent,
        IEnumerable<AttachmentOperation>? AttachmentOperations = default)
    : ICommand<EditChatMessageReplyResult>;

internal sealed class EditChatMessageReplyHandler
    : ICommandHandler<EditChatMessageReply, EditChatMessageReplyResult>
{
    private readonly IChatGroupMemberRepository _members;
    private readonly AttachmentOperationHandler _attachmentOperationHandler;
    private readonly IIdentityContext _identityContext;
    private readonly IDomainRepository<ChatMessageReply, Guid> _messageReplies;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IClock _clock;

    public EditChatMessageReplyHandler(
        IChatGroupMemberRepository members,
        IIdentityContext identityContext,
        IDomainRepository<ChatMessageReply, Guid> messageReplies,
        IEventDispatcher eventDispatcher,
        IClock clock,
        AttachmentOperationHandler attachmentOperationHandler)
    {
        _members = members;
        _identityContext = identityContext;
        _messageReplies = messageReplies;
        _eventDispatcher = eventDispatcher;
        _clock = clock;
        _attachmentOperationHandler = attachmentOperationHandler;
    }

    public async Task<EditChatMessageReplyResult> HandleAsync(
        EditChatMessageReply command,
        CancellationToken cancellationToken = default)
    {
        var replyMessage = await _messageReplies.GetAsync(command.MessageId, cancellationToken);
        if ( replyMessage is null ) return new MessageNotFoundError(command.MessageId);
        if ( replyMessage.UserId != _identityContext.Id )
            return new UserIsNotMessageSenderError(replyMessage.Id, _identityContext.Id);

        await _messageReplies.UpdateAsync(replyMessage.Id, async chatMessage =>
        {
            chatMessage.UpdatedAt = _clock.Now;
            chatMessage.Content = command.NewContent;
            if ( command.AttachmentOperations is not null )
            {
                await _attachmentOperationHandler
                    .HandleAsync(replyMessage, command.AttachmentOperations, cancellationToken);
            }
        }, cancellationToken);

        await _eventDispatcher.PublishAsync(new ChatMessageReplyEditedEvent
        {
            MessageId = replyMessage.Id,
            ReplyToId = replyMessage.ReplyToId,
            NewContent = command.NewContent,
            UserId = _identityContext.Id,
            Timestamp = _clock.Now,
            GroupId = replyMessage.ChatGroupId,
        }, cancellationToken);

        return Unit.Default;
    }
}
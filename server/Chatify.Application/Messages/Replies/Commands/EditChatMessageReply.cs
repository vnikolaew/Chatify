using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Contracts;
using Chatify.Application.Messages.Commands;
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

namespace Chatify.Application.Messages.Replies.Commands;

using EditChatMessageReplyResult = OneOf<MessageNotFoundError, UserIsNotMessageSenderError, Unit>;

public record EditChatMessageReply(
    [Required] Guid GroupId,
    [Required] Guid MessageId,
    [Required] string NewContent,
    IEnumerable<AttachmentOperation>? AttachmentOperations = default)
    : ICommand<EditChatMessageReplyResult>;

internal sealed class EditChatMessageReplyHandler(
    IChatGroupMemberRepository members,
    IIdentityContext identityContext,
    IDomainRepository<ChatMessageReply, Guid> messageReplies,
    IEventDispatcher eventDispatcher,
    IClock clock,
    IAttachmentOperationHandler attachmentOperationHandler)
    : ICommandHandler<EditChatMessageReply, EditChatMessageReplyResult>
{
    public async Task<EditChatMessageReplyResult> HandleAsync(
        EditChatMessageReply command,
        CancellationToken cancellationToken = default)
    {
        var replyMessage = await messageReplies.GetAsync(command.MessageId, cancellationToken);
        if ( replyMessage is null ) return new MessageNotFoundError(command.MessageId);
        if ( replyMessage.UserId != identityContext.Id )
            return new UserIsNotMessageSenderError(replyMessage.Id, identityContext.Id);

        await messageReplies.UpdateAsync(replyMessage.Id, async chatMessage =>
        {
            chatMessage.UpdatedAt = clock.Now;
            chatMessage.Content = command.NewContent;
            if ( command.AttachmentOperations is not null )
            {
                var attachmentOperations = command
                    .AttachmentOperations
                    .Select(o =>
                        o is AddAttachmentOperation ao
                            ? ao with { Location = FolderConstants.ChatGroups.Media.Root }
                            : o);

                await attachmentOperationHandler
                    .HandleAsync(replyMessage, attachmentOperations, cancellationToken);
            }
        }, cancellationToken);

        await eventDispatcher.PublishAsync(new ChatMessageReplyEditedEvent
        {
            MessageId = replyMessage.Id,
            ReplyToId = replyMessage.ReplyToId,
            NewContent = command.NewContent,
            UserId = identityContext.Id,
            Timestamp = clock.Now,
            GroupId = replyMessage.ChatGroupId,
        }, cancellationToken);

        return Unit.Default;
    }
}
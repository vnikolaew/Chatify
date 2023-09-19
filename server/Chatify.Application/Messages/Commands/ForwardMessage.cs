using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Contracts;
using Chatify.Application.Common.Models;
using Chatify.Application.Messages.Commands.Common;
using Chatify.Application.Messages.Common;
using Chatify.Application.Messages.Contracts;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Messages;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using OneOf;

namespace Chatify.Application.Messages.Commands;

using ForwardMessageResult =
    OneOf<MessageNotFoundError, UserIsNotMessageSenderError, ChatGroupNotFoundError, UserIsNotMemberError, Unit>;

public record ForwardMessage(
    [Required] Guid MessageId,
    [Required] List<Guid> GroupIds,
    [Required, MinLength(1), MaxLength(500)]
    string Content,
    IEnumerable<InputFile>? Attachments = default
) : SendChatMessageBase<ForwardMessageResult>(Content, Attachments);

internal sealed class ForwardMessageHandler(
        IIdentityContext identityContext,
        IMessageContentNormalizer contentNormalizer,
        IFileUploadService fileUploadService,
        IClock clock,
        IChatGroupRepository groups,
        IChatGroupMemberRepository members,
        IChatMessageRepository messages,
        IGuidGenerator guidGenerator,
        IEventDispatcher eventDispatcher)
    : SendChatMessageBaseHandler<ForwardMessage, ForwardMessageResult>(fileUploadService, identityContext)
{
    public override async Task<ForwardMessageResult> HandleAsync(
        ForwardMessage command,
        CancellationToken cancellationToken = default)
    {
        var message = await messages.GetAsync(command.MessageId, cancellationToken);
        if ( message is null ) return new MessageNotFoundError(command.MessageId);

        foreach ( var groupId in command.GroupIds )
        {
            var forwardToGroup = await groups.GetAsync(groupId, cancellationToken);
            if ( forwardToGroup is null ) return new ChatGroupNotFoundError();

            var isMember = await members.Exists(forwardToGroup.Id, identityContext.Id, cancellationToken);
            if ( !isMember ) return new UserIsNotMemberError(identityContext.Id, forwardToGroup.Id);

            // Handle file uploads:
            var uploadedFileResults = await HandleFileUploads(
                command.Attachments,
                cancellationToken);
            var attachments = GetMediae(uploadedFileResults);

            var messageId = guidGenerator.New();
            var forwardedMessage = new ChatMessage
            {
                Id = messageId,
                Attachments = attachments,
                UserId = identityContext.Id,
                ChatGroup = forwardToGroup,
                Content = contentNormalizer.Normalize(command.Content),
                CreatedAt = clock.Now,
                ForwardedMessageId = message.Id,
                ChatGroupId = forwardToGroup.Id
            };

            await messages.SaveAsync(forwardedMessage, cancellationToken);
            await eventDispatcher.PublishAsync(new ChatMessageSentEvent
            {
                UserId = forwardedMessage.UserId,
                Content = forwardedMessage.Content,
                GroupId = forwardedMessage.ChatGroupId,
                Timestamp = forwardedMessage.CreatedAt.DateTime,
                MessageId = forwardedMessage.Id,
                Attachments = forwardedMessage.Attachments.ToList()
            }, cancellationToken);
        }

        return Unit.Default;
    }
}
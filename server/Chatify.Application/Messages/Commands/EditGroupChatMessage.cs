using System.ComponentModel.DataAnnotations;
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
using LanguageExt;
using OneOf;
using MessageNotFoundError = Chatify.Application.Messages.Common.MessageNotFoundError;

namespace Chatify.Application.Messages.Commands;

using EditGroupChatMessageResult = OneOf<MessageNotFoundError, UserIsNotMessageSenderError, Unit>;

public abstract record AttachmentOperation;

public record AddAttachmentOperation(InputFile InputFile) : AttachmentOperation;

public record DeleteAttachmentOperation(Guid AttachmentId) : AttachmentOperation;

public record EditGroupChatMessage(
        [Required] Guid GroupId,
        [Required] Guid MessageId,
        [Required] string NewContent,
        IEnumerable<AttachmentOperation>? AttachmentOperations = default)
    : ICommand<EditGroupChatMessageResult>;

internal sealed class EditGroupChatMessageHandler(IIdentityContext identityContext,
        IDomainRepository<ChatMessage, Guid> messages,
        IEventDispatcher eventDispatcher,
        IClock clock,
        IAttachmentOperationHandler attachmentOperationHandler)
    : ICommandHandler<EditGroupChatMessage, EditGroupChatMessageResult>
{
    public async Task<EditGroupChatMessageResult> HandleAsync(
        EditGroupChatMessage command,
        CancellationToken cancellationToken = default)
    {
        var message = await messages.GetAsync(command.MessageId, cancellationToken);
        if ( message is null ) return new MessageNotFoundError(command.MessageId);
        if ( message.UserId != identityContext.Id )
            return new UserIsNotMessageSenderError(message.Id, identityContext.Id);

        await messages.UpdateAsync(message, async chatMessage =>
        {
            chatMessage.UpdatedAt = clock.Now;
            chatMessage.Content = command.NewContent;

            if ( command.AttachmentOperations?.Any() ?? false)
            {
                await attachmentOperationHandler
                    .HandleAsync(message, command.AttachmentOperations, cancellationToken);
            }
        }, cancellationToken);

        await eventDispatcher.PublishAsync(new ChatMessageEditedEvent
        {
            MessageId = message.Id,
            NewContent = command.NewContent,
            UserId = identityContext.Id,
            Timestamp = clock.Now,
            GroupId = message.ChatGroupId,
        }, cancellationToken);

        return Unit.Default;
    }
}
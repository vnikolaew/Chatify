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

internal sealed class EditGroupChatMessageHandler
    : ICommandHandler<EditGroupChatMessage, EditGroupChatMessageResult>
{
    private readonly IIdentityContext _identityContext;
    private readonly IAttachmentOperationHandler _attachmentOperationHandler;
    private readonly IDomainRepository<ChatMessage, Guid> _messages;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IClock _clock;

    public EditGroupChatMessageHandler(
        IIdentityContext identityContext,
        IDomainRepository<ChatMessage, Guid> messages,
        IEventDispatcher eventDispatcher, IClock clock,
        IAttachmentOperationHandler attachmentOperationHandler)
    {
        _identityContext = identityContext;
        _messages = messages;
        _eventDispatcher = eventDispatcher;
        _clock = clock;
        _attachmentOperationHandler = attachmentOperationHandler;
    }

    public async Task<EditGroupChatMessageResult> HandleAsync(
        EditGroupChatMessage command,
        CancellationToken cancellationToken = default)
    {
        var message = await _messages.GetAsync(command.MessageId, cancellationToken);
        if ( message is null ) return new MessageNotFoundError(command.MessageId);
        if ( message.UserId != _identityContext.Id )
            return new UserIsNotMessageSenderError(message.Id, _identityContext.Id);

        await _messages.UpdateAsync(message.Id, async chatMessage =>
        {
            chatMessage.UpdatedAt = _clock.Now;
            chatMessage.Content = command.NewContent;

            if ( command.AttachmentOperations?.Any() ?? false)
            {
                await _attachmentOperationHandler
                    .HandleAsync(message, command.AttachmentOperations, cancellationToken);
            }
        }, cancellationToken);

        await _eventDispatcher.PublishAsync(new ChatMessageEditedEvent
        {
            MessageId = message.Id,
            NewContent = command.NewContent,
            UserId = _identityContext.Id,
            Timestamp = _clock.Now,
            GroupId = message.ChatGroupId,
        }, cancellationToken);

        return Unit.Default;
    }
}
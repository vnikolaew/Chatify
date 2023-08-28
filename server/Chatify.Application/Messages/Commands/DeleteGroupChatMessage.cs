using System.ComponentModel.DataAnnotations;
using Chatify.Application.Messages.Common;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Messages;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using OneOf;

namespace Chatify.Application.Messages.Commands;

using DeleteGroupChatMessageResult = OneOf<MessageNotFoundError, UserIsNotMessageSenderError, Unit>;

public record DeleteGroupChatMessage(
    [Required] Guid GroupId,
    [Required] Guid MessageId
) : ICommand<DeleteGroupChatMessageResult>;

internal sealed class DeleteGroupChatMessageHandler(IIdentityContext identityContext,
        IDomainRepository<ChatMessage, Guid> messages,
        IEventDispatcher eventDispatcher,
        IClock clock)
    : ICommandHandler<DeleteGroupChatMessage, DeleteGroupChatMessageResult>
{
    public async Task<DeleteGroupChatMessageResult> HandleAsync(
        DeleteGroupChatMessage command,
        CancellationToken cancellationToken = default)
    {
        var message = await messages.GetAsync(command.MessageId, cancellationToken);
        if ( message is null ) return new MessageNotFoundError(command.MessageId);
        if ( message.UserId != identityContext.Id )
            return new UserIsNotMessageSenderError(message.Id, identityContext.Id);

        // Now delete message and then all its replies ...
        var success = await messages.DeleteAsync(message.Id, cancellationToken);
        await eventDispatcher.PublishAsync(new ChatMessageDeletedEvent
        {
            MessageId = message.Id,
            GroupId = message.ChatGroupId,
            UserId = message.UserId,
            Timestamp = clock.Now
        }, cancellationToken);

        return Unit.Default;
    }
}
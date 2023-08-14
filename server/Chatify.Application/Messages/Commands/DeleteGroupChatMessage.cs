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

internal sealed class DeleteGroupChatMessageHandler
    : ICommandHandler<DeleteGroupChatMessage, DeleteGroupChatMessageResult>
{
    private readonly IIdentityContext _identityContext;
    private readonly IDomainRepository<ChatMessage, Guid> _messages;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IClock _clock;

    public DeleteGroupChatMessageHandler(
        IIdentityContext identityContext,
        IDomainRepository<ChatMessage, Guid> messages,
        IEventDispatcher eventDispatcher,
        IClock clock)
    {
        _identityContext = identityContext;
        _messages = messages;
        _eventDispatcher = eventDispatcher;
        _clock = clock;
    }

    public async Task<DeleteGroupChatMessageResult> HandleAsync(
        DeleteGroupChatMessage command,
        CancellationToken cancellationToken = default)
    {
        var message = await _messages.GetAsync(command.MessageId, cancellationToken);
        if ( message is null ) return new MessageNotFoundError(command.MessageId);
        if ( message.UserId != _identityContext.Id )
            return new UserIsNotMessageSenderError(message.Id, _identityContext.Id);

        // Now delete message and then all its replies ...
        var success = await _messages.DeleteAsync(message.Id, cancellationToken);
        await _eventDispatcher.PublishAsync(new ChatMessageDeletedEvent
        {
            MessageId = message.Id,
            GroupId = message.ChatGroupId,
            UserId = message.UserId,
            Timestamp = _clock.Now
        }, cancellationToken);

        return Unit.Default;
    }
}
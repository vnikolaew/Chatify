using System.ComponentModel.DataAnnotations;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Messages;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using LanguageExt.Common;

namespace Chatify.Application.Messages.Commands;

using DeleteGroupChatMessageResult = Either<Error, Unit>;

public record DeleteGroupChatMessage(
    [Required] Guid GroupId,
    [Required] Guid MessageId
) : ICommand<DeleteGroupChatMessageResult>;

internal sealed class DeleteGroupChatMessageHandler
    : ICommandHandler<DeleteGroupChatMessage, DeleteGroupChatMessageResult>
{
    private readonly IChatGroupMemberRepository _members;
    private readonly IIdentityContext _identityContext;
    private readonly IDomainRepository<ChatMessage, Guid> _messages;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IClock _clock;

    public DeleteGroupChatMessageHandler(
        IChatGroupMemberRepository members,
        IIdentityContext identityContext,
        IDomainRepository<ChatMessage, Guid> messages,
        IEventDispatcher eventDispatcher,
        IClock clock)
    {
        _members = members;
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
        if (message is null) return Error.New("");
        if (message.UserId != _identityContext.Id) return Error.New("");

        // Now delete message and then all its replies ...
        var success = await _members.DeleteAsync(message.Id, cancellationToken);
        await _eventDispatcher.PublishAsync(new ChatMessageDeletedEvent
        {
            MessageId = message.Id,
            GroupId = message.ChatGroupId,
            UserId = message.UserId,
            Timestamp = _clock.Now
        }, cancellationToken);

        return success ? Unit.Default : Error.New("");
    }
}
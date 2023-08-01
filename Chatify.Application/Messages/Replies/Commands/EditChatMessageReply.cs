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

namespace Chatify.Application.Messages.Replies.Commands;

using EditChatMessageReplyResult = Either<Error, Unit>;

public record EditChatMessageReply(
    [Required] Guid GroupId,
    [Required] Guid MessageId,
    [Required] string NewContent
) : ICommand<EditChatMessageReplyResult>;

internal sealed class EditChatMessageReplyHandler
    : ICommandHandler<EditChatMessageReply, EditChatMessageReplyResult>
{
    private readonly IChatGroupMemberRepository _members;
    private readonly IIdentityContext _identityContext;
    private readonly IDomainRepository<ChatMessageReply, Guid> _messageReplies;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IClock _clock;

    public EditChatMessageReplyHandler(
        IChatGroupMemberRepository members,
        IIdentityContext identityContext,
        IDomainRepository<ChatMessageReply, Guid> messageReplies,
        IEventDispatcher eventDispatcher, IClock clock)
    {
        _members = members;
        _identityContext = identityContext;
        _messageReplies = messageReplies;
        _eventDispatcher = eventDispatcher;
        _clock = clock;
    }

    public async Task<EditChatMessageReplyResult> HandleAsync(
        EditChatMessageReply command,
        CancellationToken cancellationToken = default)
    {
        var replyMessage = await _messageReplies.GetAsync(command.MessageId, cancellationToken);
        if (replyMessage is null) return Error.New("");
        if (replyMessage.UserId != _identityContext.Id) return Error.New("");

        await _messageReplies.UpdateAsync(replyMessage.Id, chatMessage =>
        {
            chatMessage.UpdatedAt = _clock.Now;
            chatMessage.Content = command.NewContent;
        }, cancellationToken);
        
        await _eventDispatcher.PublishAsync(new ChatMessageEditedEvent
        {
            MessageId = replyMessage.Id,
            NewContent = command.NewContent,
            UserId = _identityContext.Id,
            Timestamp = _clock.Now,
            GroupId = replyMessage.ChatGroupId,
        }, cancellationToken);
        
        return Unit.Default;
    }
}
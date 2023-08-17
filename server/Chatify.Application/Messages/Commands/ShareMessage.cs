using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Contracts;
using Chatify.Application.Messages.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Messages;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using OneOf;

namespace Chatify.Application.Messages.Commands;

using ShareMessageResult =
    OneOf<MessageNotFoundError, UserIsNotMessageSenderError, ChatGroupNotFoundError, UserIsNotMemberError, Unit>;

public record ShareMessage(
    [Required] Guid MessageId,
    [Required] Guid GroupId,
    [Required, MinLength(1), MaxLength(500)]
    string Content
) : ICommand<ShareMessageResult>;

internal sealed class ShareMessageHandler : ICommandHandler<ShareMessage, ShareMessageResult>
{
    private readonly IIdentityContext _identityContext;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IClock _clock;
    private readonly IChatGroupRepository _groups;
    private readonly IChatMessageRepository _messages;
    private readonly IChatGroupMemberRepository _members;
    
    public const string SharedMessageIdKey = "shared-message-id";

    public ShareMessageHandler(IIdentityContext identityContext, IClock clock, IChatGroupRepository groups,
        IChatGroupMemberRepository members, IChatMessageRepository messages, IGuidGenerator guidGenerator,
        IEventDispatcher eventDispatcher)
    {
        _identityContext = identityContext;
        _clock = clock;
        _groups = groups;
        _members = members;
        _messages = messages;
        _guidGenerator = guidGenerator;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<ShareMessageResult> HandleAsync(
        ShareMessage command,
        CancellationToken cancellationToken = default)
    {
        var message = await _messages.GetAsync(command.MessageId, cancellationToken);
        if ( message is null ) return new MessageNotFoundError(command.MessageId);

        if ( message.UserId != _identityContext.Id )
            return new UserIsNotMessageSenderError(message.Id, _identityContext.Id);

        var forwardToGroup = await _groups.GetAsync(command.GroupId, cancellationToken);
        if ( forwardToGroup is null ) return new ChatGroupNotFoundError();

        var isMember = await _members.Exists(forwardToGroup.Id, _identityContext.Id, cancellationToken);
        if ( !isMember ) return new UserIsNotMemberError(_identityContext.Id, forwardToGroup.Id);

        var messageId = _guidGenerator.New();
        var forwardedMessage = new ChatMessage
        {
            Id = messageId,
            UserId = _identityContext.Id,
            ChatGroup = forwardToGroup,
            Content = command.Content,
            CreatedAt = _clock.Now,
            Metadata = new Dictionary<string, string> { { SharedMessageIdKey, message.Id.ToString() } },
            ChatGroupId = forwardToGroup.Id
        };

        await _messages.SaveAsync(forwardedMessage, cancellationToken);
        await _eventDispatcher.PublishAsync(new ChatMessageSentEvent
        {
            UserId = forwardedMessage.UserId,
            Content = forwardedMessage.Content,
            GroupId = forwardedMessage.ChatGroupId,
            Timestamp = forwardedMessage.CreatedAt.DateTime,
            MessageId = forwardedMessage.Id
        }, cancellationToken);

        return Unit.Default;
    }
}
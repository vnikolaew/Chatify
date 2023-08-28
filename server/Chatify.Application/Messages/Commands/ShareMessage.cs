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

internal sealed class ShareMessageHandler(IIdentityContext identityContext,
        IClock clock,
        IChatGroupRepository groups,
        IChatGroupMemberRepository members,
        IChatMessageRepository messages,
        IGuidGenerator guidGenerator,
        IEventDispatcher eventDispatcher)
    : ICommandHandler<ShareMessage, ShareMessageResult>
{
    public const string SharedMessageIdKey = "shared-message-id";

    public async Task<ShareMessageResult> HandleAsync(
        ShareMessage command,
        CancellationToken cancellationToken = default)
    {
        var message = await messages.GetAsync(command.MessageId, cancellationToken);
        if ( message is null ) return new MessageNotFoundError(command.MessageId);

        if ( message.UserId != identityContext.Id )
            return new UserIsNotMessageSenderError(message.Id, identityContext.Id);

        var forwardToGroup = await groups.GetAsync(command.GroupId, cancellationToken);
        if ( forwardToGroup is null ) return new ChatGroupNotFoundError();

        var isMember = await members.Exists(forwardToGroup.Id, identityContext.Id, cancellationToken);
        if ( !isMember ) return new UserIsNotMemberError(identityContext.Id, forwardToGroup.Id);

        var messageId = guidGenerator.New();
        var forwardedMessage = new ChatMessage
        {
            Id = messageId,
            UserId = identityContext.Id,
            ChatGroup = forwardToGroup,
            Content = command.Content,
            CreatedAt = clock.Now,
            Metadata = new Dictionary<string, string> { { SharedMessageIdKey, message.Id.ToString() } },
            ChatGroupId = forwardToGroup.Id
        };

        await messages.SaveAsync(forwardedMessage, cancellationToken);
        await eventDispatcher.PublishAsync(new ChatMessageSentEvent
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
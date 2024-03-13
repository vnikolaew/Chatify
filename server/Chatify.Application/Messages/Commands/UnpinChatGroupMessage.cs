using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.Messages.Common;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using OneOf;
using ChatGroupNotFoundError = Chatify.Application.Messages.Common.ChatGroupNotFoundError;

namespace Chatify.Application.Messages.Commands;

using UnpinChatGroupMessageResult = OneOf<MessageNotFoundError, ChatGroupNotFoundError, UserIsNotGroupAdminError, Unit>;

public record UnpinChatGroupMessage(
    [Required] Guid MessageId
) : ICommand<UnpinChatGroupMessageResult>;

internal sealed class UnpinChatGroupMessageHandler(
    IIdentityContext identityContext,
    IClock clock,
    IChatGroupRepository groups,
    IChatMessageRepository messages
) : ICommandHandler<UnpinChatGroupMessage, UnpinChatGroupMessageResult>
{
    public async Task<UnpinChatGroupMessageResult> HandleAsync(
        UnpinChatGroupMessage command,
        CancellationToken cancellationToken = default)
    {
        var message = await messages.GetAsync(command.MessageId, cancellationToken);
        if ( message is null ) return new MessageNotFoundError(command.MessageId);

        var group = await groups.GetAsync(message.ChatGroupId, cancellationToken);
        if ( group is null ) return new ChatGroupNotFoundError();

        if ( !group.HasAdmin(identityContext.Id) )
            return new UserIsNotGroupAdminError(identityContext.Id, group.Id);

        await groups.UpdateAsync(group, g =>
        {
            var pinnedMessage = g.PinnedMessages
                .FirstOrDefault(m => m.MessageId == message.Id);
            if ( pinnedMessage is not null ) g.PinnedMessages.Remove(pinnedMessage);
            g.UpdatedAt = clock.Now;
        }, cancellationToken);

        return Unit.Default;
    }
}
using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.Messages.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using OneOf;
using ChatGroupNotFoundError = Chatify.Application.Messages.Common.ChatGroupNotFoundError;

namespace Chatify.Application.Messages.Commands;

using PinChatGroupMessageResult = OneOf<MessageNotFoundError, ChatGroupNotFoundError, UserIsNotGroupAdminError, Unit>;

public record PinChatGroupMessage(
    [Required] Guid MessageId
) : ICommand<PinChatGroupMessageResult>;

internal sealed class PinChatGroupMessageHandler(
    IIdentityContext identityContext,
    IClock clock,
    IChatGroupRepository groups,
    IChatMessageRepository messages)
    : ICommandHandler<PinChatGroupMessage, PinChatGroupMessageResult>
{
    public async Task<PinChatGroupMessageResult> HandleAsync(
        PinChatGroupMessage command,
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
            g.PinnedMessages.Add(new PinnedMessage(message.Id, clock.Now, identityContext.Id));
            g.UpdatedAt = clock.Now;
        }, cancellationToken);

        return Unit.Default;
    }
}
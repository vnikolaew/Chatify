using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.Messages.Common;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using LanguageExt.Common;
using OneOf;
using ChatGroupNotFoundError = Chatify.Application.Messages.Common.ChatGroupNotFoundError;

namespace Chatify.Application.Messages.Commands;

using PinChatGroupMessageResult = OneOf<MessageNotFoundError, ChatGroupNotFoundError, UserIsNotGroupAdminError, Unit>;

public record PinChatGroupMessage(
    [Required] Guid MessageId
) : ICommand<PinChatGroupMessageResult>;

internal sealed class PinChatGroupMessageHandler(IIdentityContext identityContext,
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

        if ( !group.AdminIds.Contains(identityContext.Id) ) return new UserIsNotGroupAdminError(identityContext.Id, group.Id);
        
        await groups.UpdateAsync(group, group =>
        {
            var pinnedMessageIds = JsonSerializer.Deserialize<System.Collections.Generic.HashSet<Guid>>(
                group.Metadata["pinned_message_ids"]
            ) ?? new System.Collections.Generic.HashSet<Guid>();

            pinnedMessageIds.Add(message.Id);

            group.Metadata["pinned_message_ids"] = JsonSerializer.Serialize(pinnedMessageIds);
            group.UpdatedAt = clock.Now;
        }, cancellationToken);
        
        return Unit.Default;
    }
}
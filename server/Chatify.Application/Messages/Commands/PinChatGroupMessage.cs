using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using LanguageExt.Common;
using OneOf;
using ChatGroupNotFoundError = Chatify.Application.Messages.Common.ChatGroupNotFoundError;

namespace Chatify.Application.Messages.Commands;

using PinChatGroupMessageResult = OneOf<Error, ChatGroupNotFoundError, UserIsNotGroupAdminError, Unit>;

public record PinChatGroupMessage(
    [Required] Guid MessageId
) : ICommand<PinChatGroupMessageResult>;

internal sealed class PinChatGroupMessageHandler
    : ICommandHandler<PinChatGroupMessage, PinChatGroupMessageResult>
{
    private readonly IChatGroupRepository _groups;
    private readonly IChatMessageRepository _messages;
    private readonly IIdentityContext _identityContext;
    private readonly IClock _clock;

    public PinChatGroupMessageHandler(IIdentityContext identityContext,
        IClock clock, IChatGroupRepository groups, IChatMessageRepository messages)
    {
        _identityContext = identityContext;
        _clock = clock;
        _groups = groups;
        _messages = messages;
    }

    public async Task<PinChatGroupMessageResult> HandleAsync(
        PinChatGroupMessage command,
        CancellationToken cancellationToken = default)
    {
        var message = await _messages.GetAsync(command.MessageId, cancellationToken);
        if ( message is null ) return Error.New("");
        
        var group = await _groups.GetAsync(message.ChatGroupId, cancellationToken);
        if ( group is null ) return new ChatGroupNotFoundError();

        if ( !group.AdminIds.Contains(_identityContext.Id) ) return new UserIsNotGroupAdminError(_identityContext.Id, group.Id);
        
        await _groups.UpdateAsync(group.Id, group =>
        {
            var pinnedMessageIds = JsonSerializer.Deserialize<System.Collections.Generic.HashSet<Guid>>(
                group.Metadata["pinned_message_ids"]
            ) ?? new System.Collections.Generic.HashSet<Guid>();

            pinnedMessageIds.Add(message.Id);

            group.Metadata["pinned_message_ids"] = JsonSerializer.Serialize(pinnedMessageIds);
            group.UpdatedAt = _clock.Now;
        }, cancellationToken);
        
        return Unit.Default;
    }
}
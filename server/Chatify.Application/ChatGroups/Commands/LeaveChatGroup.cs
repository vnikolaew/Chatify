using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Contracts;
using Chatify.Application.Common;
using Chatify.Domain.Events.Groups;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.ChatGroups.Commands;

using LeaveChatGroupResult = OneOf<ChatGroupNotFoundError, UserIsNotMemberError, Unit>;

public record LeaveChatGroup(
    [Required] Guid GroupId,
    [MinLength(3), MaxLength(200)] string? Reason
) : ICommand<LeaveChatGroupResult>;

internal sealed class LeaveChatGroupHandler(
    IChatGroupsService chatGroupsService,
    IClock clock,
    IIdentityContext identityContext,
    IEventDispatcher eventDispatcher)
    : BaseCommandHandler<LeaveChatGroup, LeaveChatGroupResult>(eventDispatcher, identityContext, clock)
{
    public override async Task<LeaveChatGroupResult> HandleAsync(
        LeaveChatGroup command,
        CancellationToken cancellationToken = default)
    {
        var response = await chatGroupsService
            .LeaveChatGroupAsync(new LeaveChatGroupRequest(command.GroupId, command.Reason), cancellationToken);

        if ( response.Value is Error _ ) return Unit.Default;

        // TODO: Fire an event:
        await eventDispatcher.PublishAsync(new ChatGroupMemberLeftEvent
        {
            UserId = identityContext.Id,
            GroupId = command.GroupId,
            Timestamp = clock.Now,
            Reason = command.Reason
        }, cancellationToken);

        return Unit.Default;
    }
}
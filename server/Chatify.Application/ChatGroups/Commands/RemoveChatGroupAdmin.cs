using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Contracts;
using Chatify.Application.Common;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using OneOf;

namespace Chatify.Application.ChatGroups.Commands;

using RemoveChatGroupAdminResult = OneOf<ChatGroupNotFoundError, UserIsNotMemberError, UserIsNotGroupAdminError, Unit>;

public record RemoveChatGroupAdmin(
    [Required] Guid ChatGroupId,
    [Required] Guid AdminId
) : ICommand<RemoveChatGroupAdminResult>;

internal sealed class RemoveChatGroupAdminHandler(
    IChatGroupsService chatGroupsService,
    IEventDispatcher eventDispatcher,
    IIdentityContext identityContext,
    IClock clock
) : BaseCommandHandler<RemoveChatGroupAdmin, RemoveChatGroupAdminResult>(eventDispatcher, identityContext, clock)
{
    public override async Task<RemoveChatGroupAdminResult> HandleAsync(
        RemoveChatGroupAdmin command,
        CancellationToken cancellationToken = default)
    {
        _ =
            await chatGroupsService.RemoveChatGroupAdminAsync(
                new RemoveChatGroupAdminRequest(command.ChatGroupId, command.AdminId), cancellationToken);
        return Unit.Default;
    }
}
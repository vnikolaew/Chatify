using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common;
using Chatify.Application.User.Common;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using UnstarChatGroupResult = OneOf.OneOf<Chatify.Application.User.Common.UserNotFound, Chatify.Application.ChatGroups.Commands.ChatGroupNotFoundError, LanguageExt.Unit>;

namespace Chatify.Application.ChatGroups.Commands;

public record UnstarChatGroup(
    [Required] Guid ChatGroupId
) : ICommand<UnstarChatGroupResult>;

internal sealed class UnstarChatGroupHandler(
    IEventDispatcher eventDispatcher,
    IUserRepository users,
    IIdentityContext identityContext,
    IClock clock)
    : BaseCommandHandler<UnstarChatGroup, UnstarChatGroupResult>(eventDispatcher, identityContext, clock)
{
    public override async Task<UnstarChatGroupResult> HandleAsync(UnstarChatGroup command,
        CancellationToken cancellationToken = default)
    {
        var user = await users.GetAsync(identityContext.Id, cancellationToken);

        if ( user is null ) return new UserNotFound();
        if ( !user.HasStarredGroup(command.ChatGroupId) ) return new ChatGroupNotFoundError();

        await users.UpdateAsync(user, u => u.UnstarGroup(command.ChatGroupId), cancellationToken);
        return Unit.Default;
    }
}
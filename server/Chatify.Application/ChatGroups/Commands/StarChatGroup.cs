using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common;
using Chatify.Application.User.Common;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using StarChatGroupResult =
    OneOf.OneOf<Chatify.Application.ChatGroups.Commands.ChatGroupNotFoundError, Chatify.Application.User.Common.UserNotFound, LanguageExt.Unit>;

namespace Chatify.Application.ChatGroups.Commands;

public record StarChatGroup(
    [Required] Guid ChatGroupId
) : ICommand<StarChatGroupResult>;

internal sealed class StarChatGroupHandler(
    IEventDispatcher eventDispatcher,
    IUserRepository users,
    IIdentityContext identityContext,
    IClock clock)
    : BaseCommandHandler<StarChatGroup, StarChatGroupResult>(eventDispatcher, identityContext, clock)
{
    public override async Task<StarChatGroupResult> HandleAsync(
        StarChatGroup command,
        CancellationToken cancellationToken = default)
    {
        var user = await users.GetAsync(identityContext.Id, cancellationToken);
        
        if ( user is null ) return new UserNotFound();
        if ( user.StarredChatGroups.Contains(command.ChatGroupId) ) return new ChatGroupNotFoundError();

        await users.UpdateAsync(user, u => u.StarredChatGroups.Add(command.ChatGroupId), cancellationToken);
        return Unit.Default;
    }
}
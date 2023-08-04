using System.ComponentModel.DataAnnotations;
using System.Security.Principal;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using LanguageExt.Common;

namespace Chatify.Application.Friendships.Commands;

using UnfriendUserResult = Either<Error, Unit>;

public record UnfriendUser(
    [Required] Guid UserId
) : ICommand<UnfriendUserResult>;

internal sealed class UnfriendUserHandler
    : ICommandHandler<UnfriendUser, UnfriendUserResult>
{
    private readonly IFriendshipsRepository _friendships;
    private readonly IIdentityContext _identityContext;
    private readonly IClock _clock;

    public UnfriendUserHandler(IFriendshipsRepository friendships, IIdentityContext identityContext, IClock clock)
    {
        _friendships = friendships;
        _identityContext = identityContext;
        _clock = clock;
    }

    public async Task<UnfriendUserResult> HandleAsync(
        UnfriendUser command,
        CancellationToken cancellationToken = default)
    {
        var areFriends = (await _friendships.AllFriendIdsForUser(_identityContext.Id, cancellationToken))
            .Any(id =>
                id == command.UserId);
        if(!areFriends) return Error.New("");

        var success = await _friendships.DeleteForUsers(_identityContext.Id, command.UserId, cancellationToken);
        return success ? Unit.Default : Error.New("");
    }
}
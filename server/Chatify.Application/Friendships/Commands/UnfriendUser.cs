using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Models;
using Chatify.Domain.Events.Friendships;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.Friendships.Commands;

using UnfriendUserResult = OneOf<UsersAreNotFriendsError, Error, Unit>;

public record UsersAreNotFriendsError(Guid UserId, Guid FriendId)
    : BaseError("Users are not friends.");

public record UnfriendUser(
    [Required] Guid UserId
) : ICommand<UnfriendUserResult>;

internal sealed class UnfriendUserHandler(IFriendshipsRepository friendships,
        IIdentityContext identityContext,
        IEventDispatcher eventDispatcher,
        IClock clock,
        IChatGroupRepository groups)
    : ICommandHandler<UnfriendUser, UnfriendUserResult>
{
    public async Task<UnfriendUserResult> HandleAsync(
        UnfriendUser command,
        CancellationToken cancellationToken = default)
    {
        var userFriendships = await friendships.AllFriendshipsForUser(identityContext.Id, cancellationToken);
        var friendship = userFriendships
            .FirstOrDefault(fr =>
                fr.FriendTwoId == command.UserId);
        
        if (friendship is null) return new UsersAreNotFriendsError(identityContext.Id, command.UserId);

        var success = await friendships.DeleteForUsers(identityContext.Id, command.UserId, cancellationToken);
        if ( !success ) return Error.New("");

        success = await groups.DeleteAsync(friendship.GroupId, cancellationToken);
        if ( !success ) return Error.New("");

        await eventDispatcher.PublishAsync(new UserUnfriendedEvent
        {
            UserId = command.UserId,
            Timestamp = clock.Now,
            UnfriendedById = identityContext.Id
        }, cancellationToken);

        return Unit.Default;
    }
}
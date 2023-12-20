using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Application.Common.Behaviours.Timing;
using Chatify.Application.Friendships.Commands;
using Chatify.Application.Messages.Reactions.Commands;
using Chatify.Application.User.Common;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using OneOf;

namespace Chatify.Application.User.Queries;

using FindByUserHandleResult = OneOf<UserNotFound, UserDetailsEntry>;

[Cached("user-by-handle", 60 * 5)]
public record FindByUserHandle([Required] [property: CacheKey] string UserHandle)
    : IQuery<FindByUserHandleResult>;

[Timed]
internal sealed class FindByUserHandleHandler
(IUserRepository users,
    IFriendInvitationRepository invites,
    IFriendshipsRepository friendships,
    IIdentityContext identityContext
) : IQueryHandler<FindByUserHandle, FindByUserHandleResult>
{
    public async Task<FindByUserHandleResult> HandleAsync(FindByUserHandle query,
        CancellationToken cancellationToken = default)
    {
        var user = await users.FindByUserHandle(query.UserHandle,
            cancellationToken);

        if ( user is null ) return new UserNotFound();

        var userFriendships = await friendships
            .AllFriendshipsForUser(user.Id, cancellationToken);

        var invite = await invites.ForUsersAsync(identityContext.Id, user.Id, cancellationToken);

        return new UserDetailsEntry(
            user,
            userFriendships
                .FirstOrDefault(f => f.FriendTwoId == identityContext.Id),
            invite);
    }
}
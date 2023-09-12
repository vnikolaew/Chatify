using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Application.Common.Models;
using Chatify.Application.User.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using OneOf;

namespace Chatify.Application.User.Queries;

using GetUserDetailsResult = OneOf<UserNotFound, NotFriendsError, UserDetailsEntry>;

public record UserDetailsEntry(
    Domain.Entities.User User,
    FriendsRelation? FriendsRelation = default,
    FriendInvitation? FriendInvitation = default);

public record NotFriendsError(string? Message = default) : BaseError(Message);

[Cached("user-details", 10)]
public record GetUserDetails(
    [Required] [property: CacheKey] Guid UserId
) : IQuery<GetUserDetailsResult>;

internal sealed class GetUserDetailsHandler(IIdentityContext identityContext,
        IUserRepository users,
        IFriendshipsRepository friendships,
        IFriendInvitationRepository friendInvites)
    : IQueryHandler<GetUserDetails, GetUserDetailsResult>
{
    public async Task<GetUserDetailsResult> HandleAsync(
        GetUserDetails query,
        CancellationToken cancellationToken = default)
    {
        var user = await users.GetAsync(query.UserId, cancellationToken);

        if ( user is null ) return new UserNotFound();
        if ( query.UserId == identityContext.Id ) return new UserDetailsEntry(user);

        var userFriendships = await friendships.AllFriendshipsForUser(identityContext.Id, cancellationToken);
        var friendInvite = await friendInvites.ForUsersAsync(identityContext.Id, query.UserId, cancellationToken);

        return new UserDetailsEntry(
            user,
            userFriendships.FirstOrDefault(_ => _.FriendTwoId == query.UserId),
            friendInvite);
    }
}
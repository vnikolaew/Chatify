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

internal sealed class GetUserDetailsHandler
    : IQueryHandler<GetUserDetails, GetUserDetailsResult>
{
    private readonly IIdentityContext _identityContext;
    private readonly IFriendInvitationRepository _friendInvites;
    private readonly IFriendshipsRepository _friendships;
    private readonly IUserRepository _users;

    public GetUserDetailsHandler(
        IIdentityContext identityContext,
        IUserRepository users,
        IFriendshipsRepository friendships,
        IFriendInvitationRepository friendInvites)
    {
        _identityContext = identityContext;
        _users = users;
        _friendships = friendships;
        _friendInvites = friendInvites;
    }

    public async Task<GetUserDetailsResult> HandleAsync(
        GetUserDetails query,
        CancellationToken cancellationToken = default)
    {
        var user = await _users.GetAsync(query.UserId, cancellationToken);
        if ( user is null ) return new UserNotFound();
        if ( query.UserId == _identityContext.Id ) return new UserDetailsEntry(user);

        var friendships = await _friendships.AllFriendshipsForUser(_identityContext.Id, cancellationToken);
        var friendInvite = await _friendInvites.ForUsersAsync(_identityContext.Id, query.UserId, cancellationToken);

        return new UserDetailsEntry(
            user,
            friendships.FirstOrDefault(_ => _.FriendTwoId == query.UserId),
            friendInvite);
    }
}
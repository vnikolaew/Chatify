using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Application.Common.Models;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using OneOf;

namespace Chatify.Application.User.Queries;

using GetUserDetailsResult = OneOf<UserNotFound, NotFriendsError, Domain.Entities.User>;

public record UserNotFound;

public record NotFriendsError(string? Message = default) : BaseError(Message);

[Cached("user-details", 10)]
public record GetUserDetails(
    [Required] [property: CacheKey] Guid UserId
) : IQuery<GetUserDetailsResult>;

internal sealed class GetUserDetailsHandler
    : IQueryHandler<GetUserDetails, GetUserDetailsResult>
{
    private readonly IIdentityContext _identityContext;
    private readonly IFriendshipsRepository _friendships;
    private readonly IUserRepository _users;

    public GetUserDetailsHandler(IIdentityContext identityContext, IUserRepository users,
        IFriendshipsRepository friendships)
    {
        _identityContext = identityContext;
        _users = users;
        _friendships = friendships;
    }

    public async Task<GetUserDetailsResult> HandleAsync(
        GetUserDetails query,
        CancellationToken cancellationToken = default)
    {
        bool areFriends = ( await _friendships.AllFriendIdsForUser(_identityContext.Id, cancellationToken) )
            .Any(fid => fid == query.UserId);
        if ( !areFriends ) return new NotFriendsError();

        var user = await _users.GetAsync(query.UserId, cancellationToken);
        if ( user is null ) return new UserNotFound();

        return user;
    }
}
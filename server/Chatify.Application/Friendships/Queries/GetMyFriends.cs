using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Application.Common.Models;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using OneOf;

namespace Chatify.Application.Friendships.Queries;

using GetMyFriendsResult = OneOf<BaseError, List<Domain.Entities.User>>;

[CachedByUser("friends", 30)]
public record GetMyFriends : IQuery<GetMyFriendsResult>;

internal sealed class GetMyFriendsHandler(IFriendshipsRepository friendships,
        IIdentityContext identityContext)
    : IQueryHandler<GetMyFriends, GetMyFriendsResult>
{
    public async Task<GetMyFriendsResult> HandleAsync(GetMyFriends query,
        CancellationToken cancellationToken = default)
        => await friendships.AllForUser(identityContext.Id, cancellationToken);
}
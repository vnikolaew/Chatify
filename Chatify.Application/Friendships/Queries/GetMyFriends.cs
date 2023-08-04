using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using LanguageExt;
using LanguageExt.Common;

namespace Chatify.Application.Friendships.Queries;

using GetMyFriendsResult = Either<Error, List<Domain.Entities.User>>;

public record GetMyFriends : IQuery<GetMyFriendsResult>;

internal sealed class GetMyFriendsHandler
    : IQueryHandler<GetMyFriends, GetMyFriendsResult>
{
    private readonly IFriendshipsRepository _friendships;
    private readonly IIdentityContext _identityContext;

    public GetMyFriendsHandler(
        IFriendshipsRepository friendships,
        IIdentityContext identityContext)
    {
        _friendships = friendships;
        _identityContext = identityContext;
    }

    public async Task<GetMyFriendsResult> HandleAsync(
        GetMyFriends query,
        CancellationToken cancellationToken = default)
        => await _friendships.AllForUser(_identityContext.Id, cancellationToken);
}
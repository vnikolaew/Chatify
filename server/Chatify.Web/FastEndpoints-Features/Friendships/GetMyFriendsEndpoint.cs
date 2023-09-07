using Chatify.Application.Friendships.Queries;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Extensions;
using FastEndpoints;
using GetMyFriendsResult =
    OneOf.OneOf<Chatify.Application.Common.Models.BaseError,
        System.Collections.Generic.List<Chatify.Domain.Entities.User>>;

namespace Chatify.Web.FastEndpoints_Features.Friendships;

[HttpGet]
public sealed class GetMyFriendsEndpoint : BaseFriendshipsEndpoint<EmptyRequest, IResult>
{
    public override Task<IResult> HandleAsync(EmptyRequest req,
        CancellationToken ct)
        => QueryAsync<GetMyFriends, GetMyFriendsResult>(
                new GetMyFriends(),
                ct)
            .MatchAsync(
                err => err.ToBadRequestResult(),
                Ok);
}
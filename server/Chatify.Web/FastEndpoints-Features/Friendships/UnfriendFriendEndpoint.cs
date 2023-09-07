using Chatify.Application.Friendships.Commands;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Extensions;
using FastEndpoints;
using Guid = System.Guid;
using UnfriendUserResult =
    OneOf.OneOf<Chatify.Application.Friendships.Commands.UsersAreNotFriendsError, LanguageExt.Common.Error,
        LanguageExt.Unit>;

namespace Chatify.Web.FastEndpoints_Features.Friendships;

[HttpDelete("{friendId:guid}")]
public sealed class UnfriendFriendEndpoint : BaseFriendshipsEndpoint<EmptyRequest, IResult>
{
    public override async Task<IResult> HandleAsync(EmptyRequest req,
        CancellationToken ct)
    {
        var friendId = Route<Guid>("friendId");
        return await SendAsync<UnfriendUser, UnfriendUserResult>(
                new UnfriendUser(friendId), ct)
            .MatchAsync(
                _ => _.ToBadRequestResult(),
                _ => TypedResults.BadRequest(),
                NoContent);
    }
}
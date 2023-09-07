using Chatify.Application.Friendships.Commands;
using Chatify.Web.Common;
using Chatify.Web.Extensions;
using FastEndpoints;

namespace Chatify.Web.FastEndpoints_Features.Friendships;

using AcceptFriendInvitationResult = OneOf.OneOf<FriendInviteNotFoundError, FriendInviteInvalidStateError, Guid>;

[HttpPost("accept/{inviteId:guid}")]
public sealed class AcceptFriendInviteEndpoint : BaseFriendshipsEndpoint<EmptyRequest, IResult>
{
    public override async Task<IResult> HandleAsync(EmptyRequest req,
        CancellationToken ct)
    {
        var inviteId = Route<Guid>("inviteId");
        var result = await SendAsync<AcceptFriendInvitation, AcceptFriendInvitationResult>(
            new AcceptFriendInvitation(inviteId),
            ct);

        return result.Match(
            _ => _.ToBadRequestResult(),
            _ => _.ToBadRequestResult(),
            id => ( IResult )TypedResults.Accepted(string.Empty,
                ApiResponse<object>.Success(new { id }, "Friend invitation successfully accepted.")));
    }
}
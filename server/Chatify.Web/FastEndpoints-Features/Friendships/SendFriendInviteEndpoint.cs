using Chatify.Application.Friendships.Commands;
using Chatify.Web.Common;
using FastEndpoints;
using SendFriendInvitationResult =
    OneOf.OneOf<Chatify.Application.User.Common.UserNotFound,
        Chatify.Application.Friendships.Commands.FriendInviteNotFoundError, System.Guid>;

namespace Chatify.Web.FastEndpoints_Features.Friendships;

[HttpPost("invite/{userId:guid}")]
public sealed class SendFriendInviteEndpoint : BaseFriendshipsEndpoint<EmptyRequest, IResult>
{
    public override async Task<IResult> HandleAsync(EmptyRequest req,
        CancellationToken ct)
    {
        var userId = Route<Guid>("userId");
        
        var result = await SendAsync<SendFriendInvitation, SendFriendInvitationResult>(
            new SendFriendInvitation(userId),
            ct);
        return result.Match(
            _ => ( IResult )TypedResults.NotFound(),
            _ => TypedResults.NotFound(),
            id => TypedResults.Accepted(
                string.Empty,
                ApiResponse<object>.Success(new { id }, "Friend invitation successfully sent.")));
    }
}
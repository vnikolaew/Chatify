using Chatify.Application.Friendships.Commands;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Extensions;
using FastEndpoints;
using DeclineFriendInvitationResult =
    OneOf.OneOf<Chatify.Application.Friendships.Commands.FriendInviteNotFoundError,
        Chatify.Application.Friendships.Commands.FriendInviteInvalidStateError, LanguageExt.Unit>;

namespace Chatify.Web.FastEndpoints_Features.Friendships;

[HttpPost("decline/{inviteId:guid}")]
public class DeclineFriendInviteEndpoint : BaseFriendshipsEndpoint<EmptyRequest, IResult>
{
    public override async Task<IResult> HandleAsync(EmptyRequest req,
        CancellationToken ct)
    {
        var inviteId = Route<Guid>("inviteId");
        return await SendAsync<DeclineFriendInvitation, DeclineFriendInvitationResult>(
                new DeclineFriendInvitation(inviteId),
                ct)
            .MatchAsync(
                _ => ( IResult )_.ToBadRequest(),
                _ => ( IResult )_.ToBadRequest(),
                NoContent);
    }
}
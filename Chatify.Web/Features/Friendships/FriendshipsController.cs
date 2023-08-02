using Chatify.Application.Friendships.Commands;
using Chatify.Web.Common;
using Chatify.Web.Extensions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;

namespace Chatify.Web.Features.Friendships;

using SendFriendInvitationResult = Either<Error, Guid>;
using DeclineFriendInvitationResult = Either<Error, Unit>;
using AcceptFriendInvitationResult = Either<Error, Guid>;

public class FriendshipsController : ApiController
{
    [HttpPost]
    [Route("invite/{userId:guid}")]
    public Task<IActionResult> SendFriendInvite([FromRoute] Guid userId, CancellationToken cancellationToken = default)
        => SendAsync<SendFriendInvitation, SendFriendInvitationResult>(
                new SendFriendInvitation(userId),
                cancellationToken)
            .ToAsync()
            .Match(id => Accepted(id), err => err.ToBadRequest());

    [HttpPost]
    [Route("accept/{inviteId:guid}")]
    public Task<IActionResult> AcceptFriendInvite([FromRoute] Guid inviteId,
        CancellationToken cancellationToken = default)
        => SendAsync<AcceptFriendInvitation, AcceptFriendInvitationResult>(
                new AcceptFriendInvitation(inviteId),
                cancellationToken)
            .ToAsync()
            .Match(id => Accepted(id), err => err.ToBadRequest());

    [HttpPost]
    [Route("decline/{inviteId:guid}")]
    public Task<IActionResult> DeclineFriendInvite([FromRoute] Guid inviteId,
        CancellationToken cancellationToken = default)
        => SendAsync<DeclineFriendInvitation, DeclineFriendInvitationResult>(
                new DeclineFriendInvitation(inviteId),
                cancellationToken)
            .ToAsync()
            .Match(_ => NoContent(), err => err.ToBadRequest());
}
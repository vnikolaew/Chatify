using Chatify.Application.Friendships.Commands;
using Chatify.Application.Friendships.Queries;
using Chatify.Domain.Entities;
using Chatify.Web.Common;
using Chatify.Web.Extensions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;

namespace Chatify.Web.Features.Friendships;

using SendFriendInvitationResult = Either<Error, Guid>;
using DeclineFriendInvitationResult = Either<Error, Unit>;
using AcceptFriendInvitationResult = Either<Error, Guid>;
using GetIncomingInvitationsResult = Either<Error, List<FriendInvitation>>;
using GetSentInvitationsResult = Either<Error, List<FriendInvitation>>;

public class FriendshipsController : ApiController
{
    [HttpGet]
    [Route("sent")]
    public Task<IActionResult> GetSentInvitations(CancellationToken cancellationToken = default)
        => QueryAsync<GetSentInvitations, GetSentInvitationsResult>(
                new GetSentInvitations(),
                cancellationToken)
            .ToAsync()
            .Match(invitations => Ok(new { Data = invitations }), err => err.ToBadRequest());

    [HttpGet]
    [Route("incoming")]
    public Task<IActionResult> GetIncomingInvitations(CancellationToken cancellationToken = default)
        => QueryAsync<GetIncomingInvitations, GetIncomingInvitationsResult>(
                new GetIncomingInvitations(),
                cancellationToken)
            .ToAsync()
            .Match(invitations => Ok(new { Data = invitations }), err => err.ToBadRequest());

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
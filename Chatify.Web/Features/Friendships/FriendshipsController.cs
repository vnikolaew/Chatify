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
using GetMyFriendsResult = Either<Error, List<User>>;

public class FriendshipsController : ApiController
{
    private const string SentFriendshipsEndpoint = "sent";
    private const string IncomingFriendshipsEndpoint = "incoming";

    private const string InviteEndpoint = "invite";
    private const string AcceptEndpoint = "accept";
    private const string DeclineEndpoint = "decline";

    [HttpGet]
    public Task<IActionResult> GetMyFriends(CancellationToken cancellationToken = default)
        => QueryAsync<GetMyFriends, GetMyFriendsResult>(
                new GetMyFriends(),
                cancellationToken)
            .ToAsync()
            .Match(friends => Ok(new { Data = friends }), err => err.ToBadRequest());

    [HttpGet]
    [Route(SentFriendshipsEndpoint)]
    public Task<IActionResult> GetSentInvitations(CancellationToken cancellationToken = default)
        => QueryAsync<GetSentInvitations, GetSentInvitationsResult>(
                new GetSentInvitations(),
                cancellationToken)
            .ToAsync()
            .Match(invitations => Ok(new { Data = invitations }), err => err.ToBadRequest());

    [HttpGet]
    [Route(IncomingFriendshipsEndpoint)]
    public Task<IActionResult> GetIncomingInvitations(CancellationToken cancellationToken = default)
        => QueryAsync<GetIncomingInvitations, GetIncomingInvitationsResult>(
                new GetIncomingInvitations(),
                cancellationToken)
            .ToAsync()
            .Match(invitations => Ok(new { Data = invitations }), err => err.ToBadRequest());

    [HttpPost]
    [Route($"{InviteEndpoint}/{{userId:guid}}")]
    public Task<IActionResult> SendFriendInvite([FromRoute] Guid userId, CancellationToken cancellationToken = default)
        => SendAsync<SendFriendInvitation, SendFriendInvitationResult>(
                new SendFriendInvitation(userId),
                cancellationToken)
            .ToAsync()
            .Match(id => Accepted(id), err => err.ToBadRequest());

    [HttpPost]
    [Route($"{AcceptEndpoint}/{{inviteId:guid}}")]
    public Task<IActionResult> AcceptFriendInvite([FromRoute] Guid inviteId,
        CancellationToken cancellationToken = default)
        => SendAsync<AcceptFriendInvitation, AcceptFriendInvitationResult>(
                new AcceptFriendInvitation(inviteId),
                cancellationToken)
            .ToAsync()
            .Match(id => Accepted(id), err => err.ToBadRequest());

    [HttpPost]
    [Route($"{DeclineEndpoint}/{{inviteId:guid}}")]
    public Task<IActionResult> DeclineFriendInvite([FromRoute] Guid inviteId,
        CancellationToken cancellationToken = default)
        => SendAsync<DeclineFriendInvitation, DeclineFriendInvitationResult>(
                new DeclineFriendInvitation(inviteId),
                cancellationToken)
            .ToAsync()
            .Match(_ => NoContent(), err => err.ToBadRequest());
}
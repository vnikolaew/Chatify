using Chatify.Application.Friendships.Commands;
using Chatify.Application.Friendships.Queries;
using Chatify.Application.User.Common;
using Chatify.Domain.Entities;
using Chatify.Web.Common;
using Chatify.Web.Extensions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using OneOf;

namespace Chatify.Web.Features.Friendships;

using SendFriendInvitationResult = OneOf<UserNotFound, FriendInviteNotFoundError, Guid>;
using DeclineFriendInvitationResult = OneOf<FriendInviteNotFoundError, FriendInviteInvalidStateError, Unit>;
using AcceptFriendInvitationResult = OneOf<FriendInviteNotFoundError, FriendInviteInvalidStateError, Guid>;
using UnfriendUserResult = OneOf<UsersAreNotFriendsError, Unit>;
using GetIncomingInvitationsResult = OneOf<Error, List<FriendInvitation>>;
using GetSentInvitationsResult = OneOf<Error, List<FriendInvitation>>;
using GetMyFriendsResult = OneOf<Error, List<User>>;

public class FriendshipsController : ApiController
{
    private const string SentFriendshipsEndpoint = "sent";
    private const string IncomingFriendshipsEndpoint = "incoming";

    private const string InviteEndpoint = "invite";
    
    private const string AcceptEndpoint = "accept";
    private const string DeclineEndpoint = "decline";

    [HttpGet]
    public async Task<IActionResult> GetMyFriends(CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<GetMyFriends, GetMyFriendsResult>(
            new GetMyFriends(),
            cancellationToken);
        return result.Match(
            err => err.ToBadRequest(),
            friends => Ok(new { Data = friends }));
    }

    [HttpGet]
    [Route(SentFriendshipsEndpoint)]
    public async Task<IActionResult> GetSentInvitations(
        CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<GetSentInvitations, GetSentInvitationsResult>(
            new GetSentInvitations(),
            cancellationToken);

        return result.Match(
            err => err.ToBadRequest(),
            invitations => Ok(new { Data = invitations }));
    }

    [HttpGet]
    [Route(IncomingFriendshipsEndpoint)]
    public async Task<IActionResult> GetIncomingInvitations(CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<GetIncomingInvitations, GetIncomingInvitationsResult>(
            new GetIncomingInvitations(),
            cancellationToken);
        return result.Match(
            err => err.ToBadRequest(),
            invitations => Ok(new { Data = invitations }));
    }

    [HttpPost]
    [Route($"{InviteEndpoint}/{{userId:guid}}")]
    public async Task<IActionResult> SendFriendInvite([FromRoute] Guid userId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<SendFriendInvitation, SendFriendInvitationResult>(
            new SendFriendInvitation(userId),
            cancellationToken);
        return result.Match<IActionResult>(
            _ => BadRequest(),
            _ => BadRequest(),
            id => Accepted(id));
    }

    [HttpPost]
    [Route($"{AcceptEndpoint}/{{inviteId:guid}}")]
    public async Task<IActionResult> AcceptFriendInvite([FromRoute] Guid inviteId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<AcceptFriendInvitation, AcceptFriendInvitationResult>(
            new AcceptFriendInvitation(inviteId),
            cancellationToken);

        return result.Match<IActionResult>(
            _ => BadRequest(),
            _ => BadRequest(),
            id => Accepted(id));
    }

    [HttpPost]
    [Route($"{DeclineEndpoint}/{{inviteId:guid}}")]
    public async Task<IActionResult> DeclineFriendInvite([FromRoute] Guid inviteId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<DeclineFriendInvitation, DeclineFriendInvitationResult>(
            new DeclineFriendInvitation(inviteId), cancellationToken);
        return result.Match<IActionResult>(
            _ => NotFound(),
            _ => BadRequest(),
            _ => NoContent());
    }

    [HttpDelete]
    [Route("{friendId:guid}")]
    public async Task<IActionResult> UnfriendFriend(
        [FromRoute] Guid friendId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<UnfriendUser, UnfriendUserResult>(
            new UnfriendUser(friendId), cancellationToken);
        return result.Match<IActionResult>(
            _ => BadRequest(),
            _ => NoContent());
    }
}
using System.Net;
using Chatify.Application.Common.Models;
using Chatify.Application.Friendships.Commands;
using Chatify.Application.Friendships.Queries;
using Chatify.Application.User.Common;
using Chatify.Domain.Entities;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Common;
using Chatify.Web.Common.Attributes;
using Chatify.Web.Extensions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOfExtensions = Chatify.Shared.Infrastructure.Common.Extensions.OneOfExtensions;

namespace Chatify.Web.Features.Friendships;

using SendFriendInvitationResult = OneOf<UserNotFound, FriendInviteNotFoundError, Guid>;
using DeclineFriendInvitationResult = OneOf<FriendInviteNotFoundError, FriendInviteInvalidStateError, Unit>;
using AcceptFriendInvitationResult = OneOf<FriendInviteNotFoundError, FriendInviteInvalidStateError, Guid>;
using UnfriendUserResult = OneOf<UsersAreNotFriendsError, Error, Unit>;
using GetIncomingInvitationsResult = OneOf<BaseError, List<FriendInvitation>>;
using GetSentInvitationsResult = OneOf<BaseError, List<FriendInvitation>>;
using GetMyFriendsResult = OneOf<BaseError, List<User>>;

public class FriendshipsController : ApiController
{
    private const string SentFriendshipsRoute = "sent";
    private const string IncomingFriendshipsRoute = "incoming";

    private const string InviteRoute = "invite";

    private const string AcceptRoute = "accept";
    private const string DeclineRoute = "decline";
    
    private const string SuggestionsRoute = "suggestions";

    [HttpGet]
    [ProducesBadRequestApiResponse]
    [ProducesOkApiResponse<List<User>>]
    public Task<IActionResult> GetMyFriends(
        CancellationToken cancellationToken = default)
        => OneOfExtensions.MatchAsync(QueryAsync<GetMyFriends, GetMyFriendsResult>(
                new GetMyFriends(),
                cancellationToken),
            err => err.ToBadRequest(), Ok);

    [HttpGet]
    [Route(SentFriendshipsRoute)]
    [ProducesBadRequestApiResponse]
    [ProducesOkApiResponse<List<FriendInvitation>>]
    public Task<IActionResult> GetSentInvitations(
        CancellationToken cancellationToken = default)
        => QueryAsync<GetSentInvitations, GetSentInvitationsResult>(
                new GetSentInvitations(),
                cancellationToken)
            .MatchAsync(
                err => err.ToBadRequest(),
                Ok
            );

    [HttpGet]
    [Route(IncomingFriendshipsRoute)]
    [ProducesBadRequestApiResponse]
    [ProducesOkApiResponse<List<FriendInvitation>>]
    public Task<IActionResult> GetIncomingInvitations(
        CancellationToken cancellationToken = default)
        => QueryAsync<GetIncomingInvitations, GetIncomingInvitationsResult>(
                new GetIncomingInvitations(),
                cancellationToken)
            .MatchAsync(
                err => err.ToBadRequest(),
                Ok
            );

    [HttpPost]
    [Route($"{InviteRoute}/{{userId:guid}}")]
    [ProducesNotFoundApiResponse]
    [ProducesAcceptedApiResponse<ApiResponse<object>>]
    public async Task<IActionResult> SendFriendInvite(
        [FromRoute] Guid userId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<SendFriendInvitation, SendFriendInvitationResult>(
            new SendFriendInvitation(userId),
            cancellationToken);
        return result.Match<IActionResult>(
            _ => NotFound(),
            _ => NotFound(),
            id => Accepted(ApiResponse<object>.Success(new { id }, "Friend invitation successfully sent.")));
    }

    [HttpPost]
    [Route($"{AcceptRoute}/{{inviteId:guid}}")]
    [ProducesBadRequestApiResponse]
    [ProducesAcceptedApiResponse<ApiResponse<object>>]
    public async Task<IActionResult> AcceptFriendInvite(
        [FromRoute] Guid inviteId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<AcceptFriendInvitation, AcceptFriendInvitationResult>(
            new AcceptFriendInvitation(inviteId),
            cancellationToken);

        var pd = ProblemDetailsFactory.CreateProblemDetails(
            HttpContext, ( int )HttpStatusCode.BadRequest,
            "Bad request");

        return result.Match<IActionResult>(
            _ => _.ToBadRequest(),
            _ => _.ToBadRequest(),
            id => Accepted(ApiResponse<object>.Success(new { id }, "Friend invitation successfully accepted.")));
    }

    [HttpPost]
    [Route($"{DeclineRoute}/{{inviteId:guid}}")]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesNoContentApiResponse]
    public async Task<IActionResult> DeclineFriendInvite([FromRoute] Guid inviteId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<DeclineFriendInvitation, DeclineFriendInvitationResult>(
            new DeclineFriendInvitation(inviteId), cancellationToken);
        return result.Match<IActionResult>(
            _ => _.ToBadRequest(),
            _ => _.ToBadRequest(),
            _ => NoContent());
    }

    [HttpDelete]
    [Route("{friendId:guid}")]
    [ProducesBadRequestApiResponse]
    [ProducesNoContentApiResponse]
    public async Task<IActionResult> UnfriendFriend(
        [FromRoute] Guid friendId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<UnfriendUser, UnfriendUserResult>(
            new UnfriendUser(friendId), cancellationToken);
        return result.Match<IActionResult>(
            _ => _.ToBadRequest(),
            _ => BadRequest(),
            _ => NoContent());
    }

    [HttpGet]
    [Route(SuggestionsRoute)]
    [ProducesBadRequestApiResponse]
    [ProducesNoContentApiResponse]
    public async Task<IActionResult> GetFriendSuggestions(CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<GetFriendSuggestions, List<User>>(
            new GetFriendSuggestions(), cancellationToken);
        return Ok(result);
    }
}
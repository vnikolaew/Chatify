using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.JoinRequests.Commands;
using Chatify.Application.JoinRequests.Queries;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Common;
using Chatify.Web.Common.Attributes;
using Chatify.Web.Extensions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;

namespace Chatify.Web.Features.JoinRequests;

using JoinChatGroupResult = OneOf.OneOf<ChatGroupNotFoundError, UserIsAlreadyGroupMemberError, Guid>;
using AcceptChatGroupJoinRequestResult = OneOf.OneOf<ChatGroupNotFoundError, UserIsNotGroupAdminError, Error, Guid>;
using DeclineChatGroupJoinRequestResult = OneOf.OneOf<ChatGroupNotFoundError, UserIsNotGroupAdminError, Error, Unit>;
using GetChatGroupJoinRequestsResult =
    OneOf.OneOf<ChatGroupNotFoundError, UserIsNotGroupAdminError, List<GroupJoinRequestEntry>>;

[Route("api/chatgroups")]
public class ChatGroupJoinRequestsController : ApiController
{
    [HttpPost]
    [Route("join/{groupId:guid}")]
    [ProducesBadRequestApiResponse]
    [ProducesAcceptedApiResponse<ApiResponse<object>>]
    public async Task<IActionResult> JoinChatGroup(
        [FromRoute] Guid groupId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<JoinChatGroup, JoinChatGroupResult>(
            new JoinChatGroup(groupId), cancellationToken);

        return result.Match<IActionResult>(
            _ => BadRequest(),
            _ => _.ToBadRequest(),
            id => Accepted(ApiResponse<object>.Success(new { id }, "Chat group successfully joined.")));
    }

    [HttpPost]
    [Route("accept/{requestId:guid}")]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesAcceptedApiResponse<ApiResponse<object>>]
    public async Task<IActionResult> AcceptJoinRequest(
        [FromRoute] Guid requestId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<AcceptChatGroupJoinRequest, AcceptChatGroupJoinRequestResult>(
            new AcceptChatGroupJoinRequest(requestId), cancellationToken);

        return result.Match<IActionResult>(
            _ => NotFound(),
            err => err.ToBadRequest(),
            err => err.ToBadRequest(),
            id => Accepted(ApiResponse<object>.Success(new { id }, "Join request successfully accepted.")));
    }

    [HttpPost]
    [Route("decline/{requestId:guid}")]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesAcceptedApiResponse<ApiResponse<object>>]
    public async Task<IActionResult> DeclineJoinRequest(
        [FromRoute] Guid requestId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<DeclineChatGroupJoinRequest, DeclineChatGroupJoinRequestResult>(
            new DeclineChatGroupJoinRequest(requestId), cancellationToken);

        return result.Match<IActionResult>(
            _ => NotFound(),
            err => err.ToBadRequest(),
            err => err.ToBadRequest(),
            id => Accepted(ApiResponse<object>.Success(new { id }, "Join request successfully declined.")));
    }

    [HttpGet]
    [Route("requests/{groupId:guid}")]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesOkApiResponse<List<GroupJoinRequestEntry>>]
    public Task<IActionResult> GetForChatGroup(
        [FromRoute] Guid groupId,
        CancellationToken cancellationToken = default)
        => QueryAsync<GetChatGroupJoinRequests, GetChatGroupJoinRequestsResult>(
                new GetChatGroupJoinRequests(groupId), cancellationToken)
            .MatchAsync(
                _ => NotFound(),
                err => err.ToBadRequest(),
                Ok
            );
}
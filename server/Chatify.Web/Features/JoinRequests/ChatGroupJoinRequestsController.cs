using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.JoinRequests.Commands;
using Chatify.Application.JoinRequests.Queries;
using Chatify.Web.Common;
using Chatify.Web.Extensions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;

namespace Chatify.Web.Features.JoinRequests;

using JoinChatGroupResult = OneOf.OneOf<ChatGroupNotFoundError, UserIsAlreadyGroupMemberError, Guid>;
using AcceptChatGroupJoinRequestResult = OneOf.OneOf<Error, Guid>;
using DeclineChatGroupJoinRequestResult = OneOf.OneOf<Error, Unit>;
using GetChatGroupJoinRequestsResult = OneOf.OneOf<Error, List<GroupJoinRequestEntry>>;

[Route("api/chatgroups")]
public class ChatGroupJoinRequestsController : ApiController
{
    [HttpPost]
    [Route("join/{groupId:guid}")]
    public async Task<IActionResult> JoinChatGroup(
        [FromRoute] Guid groupId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<JoinChatGroup, JoinChatGroupResult>(
            new JoinChatGroup(groupId), cancellationToken);

        return result.Match<IActionResult>(
            _ => BadRequest(),
            _ => BadRequest(),
            id => Accepted(id));
    }

    [HttpPost]
    [Route("accept/{requestId:guid}")]
    public async Task<IActionResult> AcceptJoinRequest(
        [FromRoute] Guid requestId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<AcceptChatGroupJoinRequest, AcceptChatGroupJoinRequestResult>(
            new AcceptChatGroupJoinRequest(requestId), cancellationToken);

        return result.Match<IActionResult>(
            err => err.ToBadRequest(),
            id => Accepted(id));
    }

    [HttpPost]
    [Route("decline/{requestId:guid}")]
    public async Task<IActionResult> DeclineJoinRequest(
        [FromRoute] Guid requestId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<DeclineChatGroupJoinRequest, DeclineChatGroupJoinRequestResult>(
            new DeclineChatGroupJoinRequest(requestId), cancellationToken);

        return result.Match<IActionResult>(
            err => err.ToBadRequest(),
            Accepted);
    }

    [HttpGet]
    [Route("{groupId:guid}")]
    public async Task<IActionResult> GetJoinRequests(
        [FromRoute] Guid groupId,
        CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<GetChatGroupJoinRequests, GetChatGroupJoinRequestsResult>(
            new GetChatGroupJoinRequests(groupId), cancellationToken);

        return result.Match<IActionResult>(
            err => err.ToBadRequest(),
            requests => Ok(new { Data = requests }));
    }
}
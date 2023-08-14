using Chatify.Application.Messages.Common;
using Chatify.Application.Messages.Reactions.Commands;
using Chatify.Web.Common;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using static Chatify.Web.Features.Reactions.Models.Models;

namespace Chatify.Web.Features.Reactions;

using ReactToChatMessageResult = OneOf<MessageNotFoundError, UserIsNotMemberError, Guid>;
using UnreactToChatMessageResult = OneOf<
    MessageNotFoundError,
    MessageReactionNotFoundError,
    UserHasNotReactedError,
    Unit>;
using ReactToChatMessageReplyResult = OneOf<MessageNotFoundError, UserIsNotMemberError, Guid>;
using UnreactToChatMessageReplyResult = OneOf<
    MessageNotFoundError,
    MessageReactionNotFoundError,
    UserHasNotReactedError,
    Unit>;

public class ReactionsController : ApiController
{
    [HttpPost]
    [Route("{messageId:guid}")]
    public async Task<IActionResult> ReactToGroupChatMessage(
        [FromBody] ReactToChatMessageRequest request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<ReactToChatMessage, ReactToChatMessageResult>(
            ( request with { MessageId = messageId } ).ToCommand(), cancellationToken);

        return result.Match<IActionResult>(
            _ => BadRequest(),
            _ => BadRequest(),
            id => Accepted(new { ReactionId = id }));
    }

    [HttpPost]
    [Route("replies/{messageId:guid}")]
    public async Task<IActionResult> ReactToGroupChatMessageReply(
        [FromBody] ReactToChatMessageRequest request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<ReactToChatMessageReply, ReactToChatMessageReplyResult>(
            ( request with { MessageId = messageId } ).ToReplyCommand(), cancellationToken);
        return result.Match<IActionResult>(
            _ => BadRequest(),
            _ => BadRequest(),
            id => Accepted(new { ReactionId = id }));
    }

    [HttpDelete]
    [Route("{messageReactionId:guid}")]
    public async Task<IActionResult> UnreactToGroupChatMessage(
        [FromBody] UnreactToChatMessageRequest request,
        [FromRoute] Guid messageReactionId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<UnreactToChatMessage, UnreactToChatMessageResult>(
            ( request with { MessageReactionId = messageReactionId } ).ToCommand(), cancellationToken);
        return result.Match<IActionResult>(
            _ => NotFound(),
            _ => NotFound(),
            _ => BadRequest(),
            _ => NoContent());
    }

    [HttpDelete]
    [Route("replies/{messageReactionId:guid}")]
    public async Task<IActionResult> UnreactToGroupChatMessageReply(
        [FromBody] UnreactToChatMessageRequest request,
        [FromRoute] Guid messageReactionId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<UnreactToChatMessageReply, UnreactToChatMessageReplyResult>(
            ( request with { MessageReactionId = messageReactionId } ).ToReplyCommand(), cancellationToken);
        return result.Match<IActionResult>(
            _ => NotFound(),
            _ => NotFound(),
            _ => BadRequest(),
            _ => NoContent());
    }
}
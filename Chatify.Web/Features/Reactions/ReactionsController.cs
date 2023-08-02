using Chatify.Application.Messages.Reactions.Commands;
using Chatify.Web.Common;
using Chatify.Web.Extensions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using static Chatify.Web.Features.Reactions.Models.Models;

namespace Chatify.Web.Features.Reactions;

using ReactToChatMessageResult = Either<Error, Guid>;
using UnreactToChatMessageResult = Either<Error, Unit>;
using ReactToChatMessageReplyResult = Either<Error, Guid>;
using UnreactToChatMessageReplyResult = Either<Error, Unit>;

public class ReactionsController : ApiController
{
    [HttpPost]
    [Route("{messageId:guid}")]
    public Task<IActionResult> ReactToGroupChatMessage(
        [FromBody] ReactToChatMessageRequest request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
        => SendAsync<ReactToChatMessage, ReactToChatMessageResult>(
                (request with { MessageId = messageId }).ToCommand(), cancellationToken)
            .ToAsync()
            .Match(id => Accepted(new { ReactionId = id }), err => err.ToBadRequest());

    [HttpPost]
    [Route("replies/{messageId:guid}")]
    public Task<IActionResult> ReactToGroupChatMessageReply(
        [FromBody] ReactToChatMessageRequest request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
        => SendAsync<ReactToChatMessageReply, ReactToChatMessageReplyResult>(
                (request with { MessageId = messageId }).ToReplyCommand(), cancellationToken)
            .ToAsync()
            .Match(id => Accepted(new { ReactionId = id }), err => err.ToBadRequest());

    [HttpDelete]
    [Route("{messageReactionId:guid}")]
    public Task<IActionResult> UnreactToGroupChatMessage(
        [FromBody] UnreactToChatMessageRequest request,
        [FromRoute] Guid messageReactionId,
        CancellationToken cancellationToken = default)
        => SendAsync<UnreactToChatMessage, UnreactToChatMessageResult>(
                (request with { MessageReactionId = messageReactionId }).ToCommand())
            .ToAsync()
            .Match(_ => NoContent(), err => err.ToBadRequest());

    [HttpDelete]
    [Route("replies/{messageReactionId:guid}")]
    public Task<IActionResult> UnreactToGroupChatMessageReply(
        [FromBody] UnreactToChatMessageRequest request,
        [FromRoute] Guid messageReactionId,
        CancellationToken cancellationToken = default)
        => SendAsync<UnreactToChatMessageReply, UnreactToChatMessageReplyResult>(
                (request with { MessageReactionId = messageReactionId }).ToReplyCommand(), cancellationToken)
            .ToAsync()
            .Match(_ => NoContent(), err => err.ToBadRequest());
}
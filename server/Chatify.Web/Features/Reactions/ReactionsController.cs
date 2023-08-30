using Chatify.Application.Messages.Common;
using Chatify.Application.Messages.Reactions.Commands;
using Chatify.Application.Messages.Reactions.Queries;
using Chatify.Domain.Entities;
using Chatify.Web.Common;
using Chatify.Web.Common.Attributes;
using Chatify.Web.Extensions;
using LanguageExt;
using LanguageExt.Common;
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
using GetAllForMessageResult = OneOf<Error, UserIsNotMemberError, List<ChatMessageReaction>>;

public class ReactionsController : ApiController
{
    [HttpPost]
    [Route("{messageId:guid}")]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesAcceptedApiResponse<ApiResponse<object>>]
    public async Task<IActionResult> ReactToGroupChatMessage(
        [FromBody] ReactToChatMessageRequest request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<ReactToChatMessage, ReactToChatMessageResult>(
            ( request with { MessageId = messageId } ).ToCommand(), cancellationToken);

        return result.Match<IActionResult>(
            _ => NotFound(),
            _ => _.ToBadRequest(),
            id => Accepted(ApiResponse<object>.Success(new { id }, "Successfully reacted to chat message.")));
    }

    [HttpGet]
    [Route("{messageId:guid}")]
    [ProducesBadRequestApiResponse]
    [ProducesOkApiResponse<List<ChatMessageReaction>>]
    public async Task<IActionResult> GetMessageReactions(
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<GetAllReactionsForMessage, GetAllForMessageResult>(
            new GetAllReactionsForMessage(messageId), cancellationToken);

        return result.Match<IActionResult>(
            _ => BadRequest(),
            _ => _.ToBadRequest(),
            Ok);
    }


    [HttpPost]
    [Route("replies/{messageId:guid}")]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesAcceptedApiResponse<ApiResponse<object>>]
    public async Task<IActionResult> ReactToGroupChatMessageReply(
        [FromBody] ReactToChatMessageRequest request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<ReactToChatMessageReply, ReactToChatMessageReplyResult>(
            ( request with { MessageId = messageId } ).ToReplyCommand(), cancellationToken);
        return result.Match<IActionResult>(
            _ => NotFound(),
            _ => _.ToBadRequest(),
            id => Accepted(ApiResponse<object>.Success(new { id }, "Successfully reacted to chat message reply.")));
    }

    [HttpDelete]
    [Route("{messageReactionId:guid}")]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesAcceptedApiResponse]
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
            Accepted);
    }

    [HttpDelete]
    [Route("replies/{messageReactionId:guid}")]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesAcceptedApiResponse]
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
            Accepted);
    }
}
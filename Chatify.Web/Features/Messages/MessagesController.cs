using System.Net;
using Chatify.Application.ChatGroups.Queries;
using Chatify.Application.Messages.Commands;
using Chatify.Application.Messages.Replies.Commands;
using Chatify.Application.Messages.Replies.Queries;
using Chatify.Domain.Entities;
using Chatify.Shared.Abstractions.Queries;
using Chatify.Web.Common;
using Chatify.Web.Extensions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using static Chatify.Web.Features.Messages.Models.Models;

namespace Chatify.Web.Features.Messages;

using SendGroupChatMessageResult = Either<Error, Guid>;
using ReplyToChatMessageResult = Either<Error, Guid>;
using EditGroupChatMessageResult = Either<Error, Unit>;
using EditChatMessageReplyResult = Either<Error, Unit>;
using DeleteGroupChatMessageResult = Either<Error, Unit>;
using DeleteChatMessageReplyResult = Either<Error, Unit>;
using GetMessagesByChatGroupResult = Either<Error, CursorPaged<ChatMessage>>;
using GetMessagesForChatGroupResult = Either<Error, CursorPaged<ChatMessageReply>>;

public class MessagesController : ApiController
{
    [HttpGet]
    [Route("{groupId:guid}")]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
    public Task<IActionResult> GetPaginatedMessagesByGroup(
        [FromBody] GetMessagesByChatGroupRequest request,
        [FromRoute] Guid groupId,
        CancellationToken cancellationToken = default)
        => QueryAsync<GetMessagesForChatGroup, GetMessagesByChatGroupResult>(
                (request with { GroupId = groupId }).ToCommand(), cancellationToken)
            .ToAsync()
            .Match(
                messages => Ok(new { Data = messages }),
                err => err.ToBadRequest());

    [HttpGet]
    [Route("replies/{messageId:guid}")]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
    public Task<IActionResult> GetPaginatedRepliesByMessage(
        [FromBody] GetRepliesByForMessageRequest request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
        => QueryAsync<GetRepliesByForMessage, GetMessagesForChatGroupResult>(
                (request with { MessageId = messageId }).ToCommand(), cancellationToken)
            .ToAsync()
            .Match(
                replies => Ok(new { Data = replies }),
                err => err.ToBadRequest());

    [HttpPost]
    [Route("{groupId:guid}")]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.Accepted)]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
    public Task<IActionResult> SendGroupChatMessage(
        [FromBody] SendGroupChatMessageRequest request,
        [FromRoute] Guid groupId,
        CancellationToken cancellationToken = default)
        => SendAsync<SendGroupChatMessage, SendGroupChatMessageResult>(
                (request with { ChatGroupId = groupId }).ToCommand(), cancellationToken)
            .ToAsync()
            .Match(
                id => Accepted(new { MessageId = id }),
                err => err.ToBadRequest());

    [HttpDelete]
    [Route("replies/{messageId:guid}")]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.NoContent)]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
    public Task<IActionResult> DeleteGroupChatMessageReply(
        [FromBody] DeleteGroupChatMessageRequest request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
        => SendAsync<DeleteChatMessageReply, DeleteChatMessageReplyResult>(
                (request with { MessageId = messageId }).ToReplyCommand(), cancellationToken)
            .ToAsync()
            .Match(
                _ => NoContent(),
                err => err.ToBadRequest());


    [HttpPost]
    [Route("replies/{messageId:guid}")]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.Accepted)]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
    public Task<IActionResult> SendGroupChatMessageReply(
        [FromBody] SendGroupChatMessageReplyRequest request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
        => SendAsync<ReplyToChatMessage, ReplyToChatMessageResult>(
                (request with { ReplyToId = messageId }).ToCommand(), cancellationToken)
            .ToAsync()
            .Match(
                id => Accepted(new { MessageId = id }),
                err => err.ToBadRequest());

    [HttpPut]
    [Route("{messageId:guid}")]
    [ProducesResponseType(typeof(void), (int)HttpStatusCode.Accepted)]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
    public Task<IActionResult> EditGroupChatMessage(
        [FromBody] EditGroupChatMessageRequest request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
        => SendAsync<EditGroupChatMessage, EditGroupChatMessageResult>(
                (request with { MessageId = messageId }).ToCommand(), cancellationToken)
            .ToAsync()
            .Match(
                _ => Accepted(),
                err => err.ToBadRequest());

    [HttpPut]
    [Route("replies/{messageId:guid}")]
    [ProducesResponseType(typeof(void), (int)HttpStatusCode.Accepted)]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
    public Task<IActionResult> EditGroupChatMessageReply(
        [FromBody] EditGroupChatMessageRequest request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
        => SendAsync<EditChatMessageReply, EditChatMessageReplyResult>(
                (request with { MessageId = messageId }).ToReplyCommand(), cancellationToken)
            .ToAsync()
            .Match(
                _ => Accepted(),
                err => err.ToBadRequest());


    [HttpDelete]
    [Route("{messageId:guid}")]
    [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
    public Task<IActionResult> DeleteGroupChatMessage(
        [FromBody] DeleteGroupChatMessageRequest request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
        => SendAsync<DeleteGroupChatMessage, DeleteGroupChatMessageResult>(
                (request with { MessageId = messageId }).ToCommand(), cancellationToken)
            .ToAsync()
            .Match(
                _ => NoContent(),
                err => err.ToBadRequest());
}
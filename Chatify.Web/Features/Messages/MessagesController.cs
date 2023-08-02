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
using DeleteGroupChatMessageResult = Either<Error, Unit>;
using GetMessagesByChatGroupResult = Either<Error, CursorPaged<ChatMessage>>;
using GetMessagesForChatGroupResult = Either<Error, CursorPaged<ChatMessageReply>>;

public class MessagesController : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
    public Task<IActionResult> GetPaginatedMessagesByGroup(
        [FromBody] GetMessagesByChatGroupRequest request,
        CancellationToken cancellationToken = default)
        => QueryAsync<GetMessagesForChatGroup, GetMessagesByChatGroupResult>(
                request.ToCommand(), cancellationToken)
            .ToAsync()
            .Match(
                messages => Ok(new { Data = messages }),
                err => err.ToBadRequest());

    [HttpGet]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
    public Task<IActionResult> GetPaginatedRepliesByMessage(
        [FromBody] GetRepliesByForMessageRequest request,
        CancellationToken cancellationToken = default)
        => QueryAsync<GetRepliesByForMessage, GetMessagesForChatGroupResult>(
                request.ToCommand(), cancellationToken)
            .ToAsync()
            .Match(
                replies => Ok(new { Data = replies }),
                err => err.ToBadRequest());

    [HttpPost]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.Accepted)]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
    public Task<IActionResult> SendGroupChatMessage(
        [FromBody] SendGroupChatMessageRequest request,
        CancellationToken cancellationToken = default)
        => SendAsync<SendGroupChatMessage, SendGroupChatMessageResult>(
                request.ToCommand(), cancellationToken)
            .ToAsync()
            .Match(
                id => Accepted(new { MessageId = id }),
                err => err.ToBadRequest());

    [HttpPost]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.Accepted)]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
    public Task<IActionResult> SendGroupChatMessageReply(
        [FromBody] SendGroupChatMessageReplyRequest request,
        CancellationToken cancellationToken = default)
        => SendAsync<ReplyToChatMessage, ReplyToChatMessageResult>(
                request.ToCommand(), cancellationToken)
            .ToAsync()
            .Match(
                id => Accepted(new { MessageId = id }),
                err => err.ToBadRequest());

    [HttpPut]
    [ProducesResponseType(typeof(void), (int)HttpStatusCode.Accepted)]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
    public Task<IActionResult> EditGroupChatMessage(
        [FromBody] EditGroupChatMessageRequest request,
        CancellationToken cancellationToken = default)
        => SendAsync<EditGroupChatMessage, EditGroupChatMessageResult>(
                request.ToCommand(), cancellationToken)
            .ToAsync()
            .Match(
                _ => Accepted(),
                err => err.ToBadRequest());

    [HttpDelete]
    [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)]
    public Task<IActionResult> DeleteGroupChatMessage(
        [FromBody] DeleteGroupChatMessageRequest request,
        CancellationToken cancellationToken = default)
        => SendAsync<DeleteGroupChatMessage, DeleteGroupChatMessageResult>(
                request.ToCommand(), cancellationToken)
            .ToAsync()
            .Match(
                _ => NoContent(),
                err => err.ToBadRequest());
}
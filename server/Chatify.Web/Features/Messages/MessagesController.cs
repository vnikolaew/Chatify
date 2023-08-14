using System.Net;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.ChatGroups.Queries;
using Chatify.Application.ChatGroups.Queries.Models;
using Chatify.Application.Messages.Commands;
using Chatify.Application.Messages.Common;
using Chatify.Application.Messages.Replies.Commands;
using Chatify.Application.Messages.Replies.Queries;
using Chatify.Domain.Entities;
using Chatify.Shared.Abstractions.Queries;
using Chatify.Web.Common;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using static Chatify.Web.Features.Messages.Models.Models;
using ChatGroupNotFoundError = Chatify.Application.Messages.Common.ChatGroupNotFoundError;
using UserIsNotMemberError = Chatify.Application.Messages.Common.UserIsNotMemberError;

namespace Chatify.Web.Features.Messages;

using SendGroupChatMessageResult = OneOf<ChatGroupNotFoundError, UserIsNotMemberError, Guid>;
using ReplyToChatMessageResult = OneOf<UserIsNotMemberError, MessageNotFoundError, Guid>;
using EditGroupChatMessageResult = OneOf<MessageNotFoundError, UserIsNotMessageSenderError, Unit>;
using EditChatMessageReplyResult = OneOf<MessageNotFoundError, UserIsNotMessageSenderError, Unit>;
using DeleteGroupChatMessageResult = OneOf<MessageNotFoundError, UserIsNotMessageSenderError, Unit>;
using DeleteChatMessageReplyResult = OneOf<MessageNotFoundError, UserIsNotMessageSenderError, Unit>;
using GetMessagesForChatGroupResult = OneOf<UserIsNotMemberError, CursorPaged<ChatGroupMessageEntry>>;
using PinChatGroupMessageResult = OneOf<Error, ChatGroupNotFoundError, UserIsNotGroupAdminError, Unit>;
using GetMessageRepliesForChatGroupMessageResult =
    OneOf<MessageNotFoundError, UserIsNotMemberError, CursorPaged<ChatMessageReply>>;

public class MessagesController : ApiController
{
    [HttpGet]
    [Route("{groupId:guid}")]
    [ProducesResponseType(typeof(object), ( int )HttpStatusCode.OK)]
    [ProducesResponseType(typeof(object), ( int )HttpStatusCode.BadRequest)]
    public async Task<IActionResult> GetPaginatedMessagesByGroup(
        [FromBody] GetMessagesByChatGroupRequest request,
        [FromRoute] Guid groupId,
        CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<GetMessagesForChatGroup, GetMessagesForChatGroupResult>(
            ( request with { GroupId = groupId } ).ToCommand(), cancellationToken);

        return result.Match<IActionResult>(
            BadRequest,
            messages => Ok(new { Data = messages }));
    }

    [HttpGet]
    [Route("{messageId:guid}/replies")]
    [ProducesResponseType(typeof(object), ( int )HttpStatusCode.OK)]
    [ProducesResponseType(typeof(object), ( int )HttpStatusCode.BadRequest)]
    public async Task<IActionResult> GetPaginatedRepliesByMessage(
        [FromBody] GetRepliesByForMessageRequest request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<GetRepliesForMessage, GetMessageRepliesForChatGroupMessageResult>(
            ( request with { MessageId = messageId } ).ToCommand(), cancellationToken);

        return result.Match<IActionResult>(
            _ => BadRequest(),
            _ => BadRequest(),
            replies => Ok(new { Data = replies }));
    }

    [HttpPost]
    [Route("{groupId:guid}")]
    [ProducesResponseType(typeof(object), ( int )HttpStatusCode.Accepted)]
    [ProducesResponseType(typeof(object), ( int )HttpStatusCode.BadRequest)]
    public async Task<IActionResult> SendGroupChatMessage(
        [FromForm] SendGroupChatMessageRequest request,
        [FromRoute] Guid groupId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<SendGroupChatMessage, SendGroupChatMessageResult>(
            ( request with { ChatGroupId = groupId } ).ToCommand(), cancellationToken);
        return result.Match<IActionResult>(
            _ => BadRequest(),
            _ => BadRequest(),
            id => Accepted(new { MessageId = id }));
    }

    [HttpDelete]
    [Route("replies/{messageId:guid}")]
    [ProducesResponseType(typeof(object), ( int )HttpStatusCode.NoContent)]
    [ProducesResponseType(typeof(object), ( int )HttpStatusCode.BadRequest)]
    public async Task<IActionResult> DeleteGroupChatMessageReply(
        [FromBody] DeleteGroupChatMessageRequest request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<DeleteChatMessageReply, DeleteChatMessageReplyResult>(
            ( request with { MessageId = messageId } ).ToReplyCommand(), cancellationToken);
        return result.Match(
            _ => NotFound(),
            _ => BadRequest(),
            NoContent);
    }

    [HttpPost]
    [Route("{messageId:guid}/replies")]
    [ProducesResponseType(typeof(object), ( int )HttpStatusCode.Accepted)]
    [ProducesResponseType(typeof(object), ( int )HttpStatusCode.BadRequest)]
    public async Task<IActionResult> SendGroupChatMessageReply(
        [FromBody] SendGroupChatMessageReplyRequest request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<ReplyToChatMessage, ReplyToChatMessageResult>(
            ( request with { ReplyToId = messageId } ).ToCommand(), cancellationToken);
        return result.Match<IActionResult>(
            _ => BadRequest(),
            _ => NotFound(),
            id => Accepted(new { MessageId = id }));
    }

    [HttpPut]
    [Route("{messageId:guid}")]
    [ProducesResponseType(typeof(void), ( int )HttpStatusCode.Accepted)]
    [ProducesResponseType(typeof(object), ( int )HttpStatusCode.BadRequest)]
    public async Task<IActionResult> EditGroupChatMessage(
        [FromBody] EditGroupChatMessageRequest request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<EditGroupChatMessage, EditGroupChatMessageResult>(
            ( request with { MessageId = messageId } ).ToCommand(), cancellationToken);

        return result.Match<IActionResult>(
            _ => BadRequest(),
            _ => BadRequest(),
            _ => Accepted());
    }

    [HttpPut]
    [Route("replies/{messageId:guid}")]
    [ProducesResponseType(typeof(void), ( int )HttpStatusCode.Accepted)]
    [ProducesResponseType(typeof(object), ( int )HttpStatusCode.BadRequest)]
    public async Task<IActionResult> EditGroupChatMessageReply(
        [FromBody] EditGroupChatMessageRequest request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<EditChatMessageReply, EditChatMessageReplyResult>(
            ( request with { MessageId = messageId } ).ToReplyCommand(), cancellationToken);
        return result.Match(
            _ => NotFound(),
            _ => BadRequest(),
            Accepted);
    }


    [HttpDelete]
    [Route("{messageId:guid}")]
    [ProducesResponseType(typeof(void), ( int )HttpStatusCode.NoContent)]
    [ProducesResponseType(typeof(object), ( int )HttpStatusCode.BadRequest)]
    public async Task<IActionResult> DeleteGroupChatMessage(
        [FromBody] DeleteGroupChatMessageRequest request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<DeleteGroupChatMessage, DeleteGroupChatMessageResult>(
            ( request with { MessageId = messageId } ).ToCommand(), cancellationToken);

        return result.Match<IActionResult>(
            _ => NotFound(),
            _ => BadRequest(),
            _ => NoContent());
    }
    
    [HttpPost]
    [Route("/pins/{messageId:guid}")]
    [ProducesResponseType(typeof(void), ( int )HttpStatusCode.NoContent)]
    [ProducesResponseType(typeof(object), ( int )HttpStatusCode.BadRequest)]
    public async Task<IActionResult> PinGroupChatMessage(
        [FromBody] PinChatGroupMessage request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<PinChatGroupMessage, PinChatGroupMessageResult>(
            new PinChatGroupMessage(messageId), cancellationToken);
        return result.Match<IActionResult>(
            _ => BadRequest(),
            _ => NotFound(),
            _ => BadRequest(),
            _ => NoContent()
            );
    }
}
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
using Chatify.Web.Common.Attributes;
using Chatify.Web.Extensions;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using static Chatify.Web.Features.Messages.Models.Models;
using ChatGroupNotFoundError = Chatify.Application.Messages.Common.ChatGroupNotFoundError;
using UserIsNotMemberError = Chatify.Application.Messages.Common.UserIsNotMemberError;

namespace Chatify.Web.Features.Messages;

using SendGroupChatMessageResult = OneOf<ChatGroupNotFoundError, UserIsNotMemberError, Guid>;
using ShareMessageResult =
    OneOf<MessageNotFoundError, UserIsNotMessageSenderError, ChatGroupNotFoundError, UserIsNotMemberError, Unit>;
using ReplyToChatMessageResult = OneOf<UserIsNotMemberError, MessageNotFoundError, Guid>;
using EditGroupChatMessageResult = OneOf<MessageNotFoundError, UserIsNotMessageSenderError, Unit>;
using EditChatMessageReplyResult = OneOf<MessageNotFoundError, UserIsNotMessageSenderError, Unit>;
using DeleteGroupChatMessageResult = OneOf<MessageNotFoundError, UserIsNotMessageSenderError, Unit>;
using DeleteChatMessageReplyResult = OneOf<MessageNotFoundError, UserIsNotMessageSenderError, Unit>;
using GetMessagesForChatGroupResult = OneOf<UserIsNotMemberError, CursorPaged<ChatGroupMessageEntry>>;
using PinChatGroupMessageResult = OneOf<MessageNotFoundError, ChatGroupNotFoundError, UserIsNotGroupAdminError, Unit>;
using GetMessageRepliesForChatGroupMessageResult =
    OneOf<MessageNotFoundError, UserIsNotMemberError, CursorPaged<ChatMessageReply>>;

public class MessagesController : ApiController
{
    [HttpGet]
    [Route("{groupId:guid}")]
    [ProducesBadRequestApiResponse]
    [ProducesOkApiResponse<CursorPaged<ChatGroupMessageEntry>>]
    public async Task<IActionResult> GetPaginatedMessagesByGroup(
        [FromQuery] GetMessagesByChatGroupRequest request,
        [FromRoute] Guid groupId,
        CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<GetMessagesForChatGroup, GetMessagesForChatGroupResult>(
            ( request with { GroupId = groupId } ).ToCommand(), cancellationToken);

        return result.Match( BadRequest, Ok);
    }

    [HttpGet]
    [Route("{messageId:guid}/replies")]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesOkApiResponse<CursorPaged<ChatMessageReply>>]
    public async Task<IActionResult> GetPaginatedRepliesByMessage(
        [FromQuery] GetRepliesByForMessageRequest request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<GetRepliesForMessage, GetMessageRepliesForChatGroupMessageResult>(
            ( request with { MessageId = messageId } ).ToCommand(), cancellationToken);

        return result.Match<IActionResult>(
            _ => NotFound(),
            _ => BadRequest(),
            Ok);
    }

    [HttpPost]
    [Route("{groupId:guid}")]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesAcceptedApiResponse<ApiResponse<object>>]
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
            _ => NotFound(),
            _ => BadRequest(),
            id => Accepted(ApiResponse<object>.Success(new { id }, "Chat message successfully sent.")));
    }

    [HttpDelete]
    [Route("replies/{messageId:guid}")]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesNoContentApiResponse]
    public async Task<IActionResult> DeleteGroupChatMessageReply(
        [FromBody] DeleteGroupChatMessageRequest request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<DeleteChatMessageReply, DeleteChatMessageReplyResult>(
            ( request with { MessageId = messageId } ).ToReplyCommand(), cancellationToken);
        return result.Match(
            _ => NotFound(),
            _ => _.ToBadRequest(),
            NoContent);
    }

    [HttpPost]
    [Route("{messageId:guid}/replies")]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesAcceptedApiResponse<ApiResponse<object>>]
    public async Task<IActionResult> SendGroupChatMessageReply(
        [FromBody] SendGroupChatMessageReplyRequest request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<ReplyToChatMessage, ReplyToChatMessageResult>(
            ( request with { ReplyToId = messageId } ).ToCommand(), cancellationToken);

        return result.Match<IActionResult>(
            _ => _.ToBadRequest(),
            _ => NotFound(),
            id => Accepted(ApiResponse<object>.Success(new { id }, "Chat message reply successfully sent.")));
    }

    [HttpPut]
    [Route("{messageId:guid}")]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesAcceptedApiResponse]
    public async Task<IActionResult> EditGroupChatMessage(
        [FromBody] EditGroupChatMessageRequest request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<EditGroupChatMessage, EditGroupChatMessageResult>(
            ( request with { MessageId = messageId } ).ToCommand(), cancellationToken);

        return result.Match<IActionResult>(
            _ => NotFound(),
            _ => _.ToBadRequest(),
            Accepted);
    }

    [HttpPut]
    [Route("replies/{messageId:guid}")]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesAcceptedApiResponse]
    public async Task<IActionResult> EditGroupChatMessageReply(
        [FromBody] EditGroupChatMessageRequest request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<EditChatMessageReply, EditChatMessageReplyResult>(
            ( request with { MessageId = messageId } ).ToReplyCommand(), cancellationToken);
        return result.Match(
            _ => NotFound(),
            _ => _.ToBadRequest(),
            Accepted);
    }


    [HttpDelete]
    [Route("{messageId:guid}")]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesNoContentApiResponse]
    public async Task<IActionResult> DeleteGroupChatMessage(
        [FromBody] DeleteGroupChatMessageRequest request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<DeleteGroupChatMessage, DeleteGroupChatMessageResult>(
            ( request with { MessageId = messageId } ).ToCommand(), cancellationToken);

        return result.Match<IActionResult>(
            _ => NotFound(),
            _ => _.ToBadRequest(),
            _ => NoContent());
    }

    [HttpPost]
    [Route("/pins/{messageId:guid}")]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesNoContentApiResponse]
    public async Task<IActionResult> PinGroupChatMessage(
        [FromBody] PinChatGroupMessage request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<PinChatGroupMessage, PinChatGroupMessageResult>(
            new PinChatGroupMessage(messageId), cancellationToken);
        return result.Match<IActionResult>(
            _ => NotFound(),
            _ => NotFound(),
            _ => _.ToBadRequest(),
            _ => NoContent()
        );
    }

    [HttpPost]
    [Route("/share/{messageId:guid}")]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesAcceptedApiResponse]
    public async Task<IActionResult> ShareChatMessage(
        [FromBody] ShareMessage request,
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<ShareMessage, ShareMessageResult>(
            request, cancellationToken);
        return result.Match<IActionResult>(
            _ => NotFound(),
            _ => _.ToBadRequest(),
            _ => NotFound(),
            _ => _.ToBadRequest(),
            Accepted
        );
    }
}
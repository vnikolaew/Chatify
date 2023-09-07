using Chatify.Application.Messages.Replies.Commands;
using Chatify.Web.Common;
using Chatify.Web.Extensions;
using Chatify.Web.Features.Messages.Models;
using FastEndpoints;
using ReplyToChatMessageResult =
    OneOf.OneOf<Chatify.Application.Messages.Common.UserIsNotMemberError,
        Chatify.Application.Messages.Common.MessageNotFoundError, System.Guid>;

namespace Chatify.Web.FastEndpoints_Features.Messages;

[HttpPost("{messageId:guid}/replies")]
public sealed class
    SendGroupChatMessageReplyEndpoint : BaseMessagesEndpoint<Models.SendGroupChatMessageReplyRequest, IResult>
{
    public override async Task<IResult> HandleAsync(Models.SendGroupChatMessageReplyRequest req,
        CancellationToken ct)
    {
        var messageId = Route<Guid>("messageId");
        var result = await SendAsync<ReplyToChatMessage, ReplyToChatMessageResult>(
            ( req with { ReplyToId = messageId } ).ToCommand(), ct);

        return result.Match<IResult>(
            _ => _.ToBadRequestResult(),
            _ => TypedResults.NotFound(),
            id => TypedResults.Accepted(string.Empty,
                ApiResponse<object>.Success(new { id }, "Chat message reply successfully sent.")));
    }
}
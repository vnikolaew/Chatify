using Chatify.Application.Messages.Commands;
using Chatify.Web.Common;
using Chatify.Web.Features.Messages.Models;
using SendGroupChatMessageResult =
    OneOf.OneOf<Chatify.Application.Messages.Common.ChatGroupNotFoundError,
        Chatify.Application.Messages.Common.UserIsNotMemberError, System.Guid>;

namespace Chatify.Web.FastEndpoints_Features.Messages;

[FastEndpoints.HttpPost("{groupId:guid}")]
public sealed class SendGroupChatMessageEndpoint : BaseMessagesEndpoint<Models.SendGroupChatMessageRequest, IResult>
{
    public override async Task<IResult> HandleAsync(Models.SendGroupChatMessageRequest req,
        CancellationToken ct)
    {
        var groupId = Route<Guid>("groupId");
        var result = await SendAsync<SendGroupChatMessage, SendGroupChatMessageResult>(
            ( req with { ChatGroupId = groupId } ).ToCommand(), ct);

        return result.Match<IResult>(
            _ => TypedResults.NotFound(),
            _ => TypedResults.BadRequest(),
            id => TypedResults.Accepted(string.Empty,
                ApiResponse<object>.Success(new { id }, "Chat message successfully sent.")));
    }
}
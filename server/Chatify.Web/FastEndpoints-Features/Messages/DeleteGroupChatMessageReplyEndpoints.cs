using Chatify.Application.Messages.Replies.Commands;
using Chatify.Web.Extensions;
using Chatify.Web.Features.Messages.Models;
using FastEndpoints;
using DeleteChatMessageReplyResult =
    OneOf.OneOf<Chatify.Application.Messages.Common.MessageNotFoundError,
        Chatify.Application.Messages.Common.UserIsNotMessageSenderError, LanguageExt.Unit>;

namespace Chatify.Web.FastEndpoints_Features.Messages;

[HttpDelete("replies/{messageId:guid}")]
public sealed class
    DeleteGroupChatMessageReplyEndpoint : BaseMessagesEndpoint<Models.DeleteGroupChatMessageRequest, IResult>
{
    public override async Task<IResult> HandleAsync(Models.DeleteGroupChatMessageRequest req,
        CancellationToken ct)
    {
        var messageId = Route<Guid>("messageId");
        var result = await SendAsync<DeleteChatMessageReply, DeleteChatMessageReplyResult>(
            ( req with { MessageId = messageId } ).ToReplyCommand(), ct);

        return result.Match<IResult>(
            _ => TypedResults.NotFound(),
            _ => _.ToBadRequestResult(),
            NoContent);
    }
}
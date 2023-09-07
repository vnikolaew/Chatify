using Chatify.Application.Messages.Replies.Commands;
using Chatify.Web.Extensions;
using Chatify.Web.Features.Messages.Models;
using FastEndpoints;
using EditChatMessageReplyResult =
    OneOf.OneOf<Chatify.Application.Messages.Common.MessageNotFoundError,
        Chatify.Application.Messages.Common.UserIsNotMessageSenderError, LanguageExt.Unit>;

namespace Chatify.Web.FastEndpoints_Features.Messages;

[HttpPut("replies/{messageId:guid}")]
public sealed class EditGroupChatMessageReplyEndpoint
    : BaseMessagesEndpoint<Models.EditGroupChatMessageRequest, IResult>
{
    public override async Task<IResult> HandleAsync(Models.EditGroupChatMessageRequest req,
        CancellationToken ct)
    {
        var messageId = Route<Guid>("messageId");
        var result = await SendAsync<EditChatMessageReply, EditChatMessageReplyResult>(
            ( req with { MessageId = messageId } ).ToReplyCommand(), ct);

        return result.Match<IResult>(
            _ => TypedResults.NotFound(),
            _ => _.ToBadRequestResult(),
            Accepted);
    }
}
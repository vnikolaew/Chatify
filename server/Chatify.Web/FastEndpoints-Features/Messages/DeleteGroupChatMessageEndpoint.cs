using Chatify.Application.Messages.Commands;
using Chatify.Web.Extensions;
using Chatify.Web.Features.Messages.Models;
using FastEndpoints;
using DeleteGroupChatMessageResult =
    OneOf.OneOf<Chatify.Application.Messages.Common.MessageNotFoundError,
        Chatify.Application.Messages.Common.UserIsNotMessageSenderError, LanguageExt.Unit>;

namespace Chatify.Web.FastEndpoints_Features.Messages;

[HttpDelete("{messageId:guid}")]
public sealed class DeleteGroupChatMessageEndpoint : BaseMessagesEndpoint<Models.DeleteGroupChatMessageRequest, IResult>
{
    public override async Task<IResult> HandleAsync(Models.DeleteGroupChatMessageRequest req,
        CancellationToken ct)
    {
        var messageId = Route<Guid>("messageId");
        var result = await SendAsync<DeleteGroupChatMessage, DeleteGroupChatMessageResult>(
            ( req with { MessageId = messageId } ).ToCommand(), ct);

        return result.Match<IResult>(
            _ => TypedResults.NotFound(),
            _ => _.ToBadRequestResult(),
            NoContent);
    }
}
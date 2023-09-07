using Chatify.Application.Messages.Commands;
using Chatify.Web.Extensions;
using Chatify.Web.Features.Messages.Models;
using FastEndpoints;
using EditGroupChatMessageResult =
    OneOf.OneOf<Chatify.Application.Messages.Common.MessageNotFoundError,
        Chatify.Application.Messages.Common.UserIsNotMessageSenderError, LanguageExt.Unit>;

namespace Chatify.Web.FastEndpoints_Features.Messages;

[HttpPut("{messageId:guid}")]
public sealed class EditGroupChatMessageEndpoint : BaseMessagesEndpoint<Models.EditGroupChatMessageRequest, IResult>
{
    public override async Task<IResult> HandleAsync(Models.EditGroupChatMessageRequest req,
        CancellationToken ct)
    {
        var messageId = Route<Guid>("messageId");
        var result = await SendAsync<EditGroupChatMessage, EditGroupChatMessageResult>(
            ( req with { MessageId = messageId } ).ToCommand(), ct);

        return result.Match<IResult>(
            _ => TypedResults.NotFound(),
            _ => _.ToBadRequestResult(),
            Accepted);
    }
}
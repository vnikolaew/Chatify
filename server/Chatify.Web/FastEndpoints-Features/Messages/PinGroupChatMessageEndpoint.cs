using Chatify.Application.Messages.Commands;
using Chatify.Web.Extensions;
using FastEndpoints;
using PinChatGroupMessageResult =
    OneOf.OneOf<Chatify.Application.Messages.Common.MessageNotFoundError,
        Chatify.Application.Messages.Common.ChatGroupNotFoundError,
        Chatify.Application.ChatGroups.Commands.UserIsNotGroupAdminError, LanguageExt.Unit>;

namespace Chatify.Web.FastEndpoints_Features.Messages;

[HttpPost("pins/{messageId:guid}")]
public sealed class PinGroupChatMessageEndpoint : BaseMessagesEndpoint<EmptyRequest, IResult>
{
    public override async Task<IResult> HandleAsync(EmptyRequest req,
        CancellationToken ct)
    {
        var messageId = Route<Guid>("messageId");
        var result = await SendAsync<PinChatGroupMessage, PinChatGroupMessageResult>(
            new PinChatGroupMessage(messageId), ct);

        return result.Match<IResult>(
            _ => TypedResults.NotFound(),
            _ => TypedResults.NotFound(),
            _ => _.ToBadRequestResult(),
            NoContent
        );
    }
}
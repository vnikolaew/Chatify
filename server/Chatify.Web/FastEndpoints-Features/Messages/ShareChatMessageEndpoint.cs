using Chatify.Application.Messages.Commands;
using Chatify.Application.Messages.Common;
using Chatify.Web.Extensions;
using FastEndpoints;
using LanguageExt;

namespace Chatify.Web.FastEndpoints_Features.Messages;

using ShareMessageResult =
    OneOf.OneOf<MessageNotFoundError, UserIsNotMessageSenderError, ChatGroupNotFoundError, UserIsNotMemberError, Unit>;

[HttpPost("/share/{messageId:guid}")]
public sealed class ShareChatMessageEndpoint : BaseMessagesEndpoint<ForwardMessage, IResult>
{
    public override async Task<IResult> HandleAsync(ForwardMessage req,
        CancellationToken ct)
    {
        var messageId = Route<Guid>("messageId");
        var result = await SendAsync<ForwardMessage, ShareMessageResult>(
            req with { MessageId = messageId }, ct);

        return result.Match<IResult>(
            _ => TypedResults.NotFound(),
            _ => _.ToBadRequestResult(),
            _ => TypedResults.NotFound(),
            _ => _.ToBadRequestResult(),
            Accepted
        );
    }
}
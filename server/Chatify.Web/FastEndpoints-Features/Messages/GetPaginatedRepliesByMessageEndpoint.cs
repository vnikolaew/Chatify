using Chatify.Application.Messages.Replies.Queries;
using Chatify.Web.Features.Messages.Models;
using GetMessageRepliesForChatGroupMessageResult =
    OneOf.OneOf<Chatify.Application.Messages.Common.MessageNotFoundError,
        Chatify.Application.Messages.Common.UserIsNotMemberError, Chatify.Shared.Abstractions.Queries.CursorPaged<
            Chatify.Domain.Entities.ChatMessageReply>>;

namespace Chatify.Web.FastEndpoints_Features.Messages;

[FastEndpoints.HttpGet("{messageId:guid}/replies")]
public sealed class
    GetPaginatedRepliesByMessageEndpoint : BaseMessagesEndpoint<Models.GetRepliesByForMessageRequest, IResult>
{
    public override async Task<IResult> HandleAsync(Models.GetRepliesByForMessageRequest req,
        CancellationToken ct)
    {
        var messageId = Route<Guid>("messageId");

        var result = await QueryAsync<GetRepliesForMessage, GetMessageRepliesForChatGroupMessageResult>(
            ( req with { MessageId = messageId } ).ToCommand(), ct);

        return result.Match(
            _ => TypedResults.NotFound(),
            _ => TypedResults.NotFound(),
            Ok);
    }
}
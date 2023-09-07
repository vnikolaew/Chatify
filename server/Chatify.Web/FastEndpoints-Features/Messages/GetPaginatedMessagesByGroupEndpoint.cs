using Chatify.Application.ChatGroups.Queries;
using Chatify.Web.Features.Messages.Models;
using FastEndpoints;
using GetMessagesForChatGroupResult =
    OneOf.OneOf<Chatify.Application.Messages.Common.UserIsNotMemberError, Chatify.Shared.Abstractions.Queries.
        CursorPaged<Chatify.Application.ChatGroups.Queries.Models.ChatGroupMessageEntry>>;

namespace Chatify.Web.FastEndpoints_Features.Messages;

[HttpGet("{groupId:guid}")]
public sealed class
    GetPaginatedMessagesByGroupEndpoint : BaseMessagesEndpoint<Models.GetMessagesByChatGroupRequest, IResult>
{
    public override async Task<IResult> HandleAsync(
        Models.GetMessagesByChatGroupRequest req,
        CancellationToken ct)
    {
        var groupId = Route<Guid>("groupId");
        var result = await QueryAsync<GetMessagesForChatGroup, GetMessagesForChatGroupResult>(
            ( req with { GroupId = groupId } ).ToCommand(), ct);

        return result.Match(_ => TypedResults.BadRequest(), Ok);
    }
}
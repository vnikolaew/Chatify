using Chatify.Application.Messages.Queries;
using Chatify.Web.Extensions;
using FastEndpoints;
using GetChatGroupPinnedMessagesResult =
    OneOf.OneOf<Chatify.Application.Messages.Common.ChatGroupNotFoundError,
        Chatify.Application.ChatGroups.Commands.UserIsNotMemberError,
        System.Collections.Generic.List<Chatify.Domain.Entities.ChatMessage>>;

namespace Chatify.Web.FastEndpoints_Features.ChatGroups;

[HttpGet("{groupId:guid}/pins")]
public sealed class PinnedMessagesEndpoint : BaseChatGroupsEndpoint<GetChatGroupPinnedMessages, IResult>
{
    public override async Task<IResult> HandleAsync(GetChatGroupPinnedMessages req,
        CancellationToken ct)
    {
        var groupId = Route<Guid>("groupId");

        var result = await QueryAsync<GetChatGroupPinnedMessages, GetChatGroupPinnedMessagesResult>(
            new GetChatGroupPinnedMessages(groupId), ct);

        return result.Match(
            _ => TypedResults.NotFound(),
            _ => _.ToBadRequestResult(),
            Ok);
    }
}
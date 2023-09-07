using Chatify.Application.ChatGroups.Queries;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Extensions;
using FastEndpoints;
using GetChatGroupDetailsResult =
    OneOf.OneOf<Chatify.Application.ChatGroups.Commands.ChatGroupNotFoundError,
        Chatify.Application.ChatGroups.Commands.UserIsNotMemberError,
        Chatify.Application.ChatGroups.Queries.ChatGroupDetailsEntry>;

namespace Chatify.Web.FastEndpoints_Features.ChatGroups;

[HttpGet("{groupId:guid}")]
public sealed class DetailsEndpoint : BaseChatGroupsEndpoint<EmptyRequest, IResult>
{
    public override Task<IResult> HandleAsync(EmptyRequest req,
        CancellationToken ct)
    {
        var groupId = Route<Guid>("groupId");

        return QueryAsync<GetChatGroupDetails, GetChatGroupDetailsResult>(
                new GetChatGroupDetails(groupId),
                ct)
            .MatchAsync(_ => TypedResults.NotFound(),
                _ => _.ToBadRequestResult(),
                Ok);
    }
}
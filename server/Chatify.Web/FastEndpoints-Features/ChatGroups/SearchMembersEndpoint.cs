using Chatify.Application.ChatGroups.Queries;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Extensions;
using FastEndpoints;
using SearchChatGroupMembersByNameResult =
    OneOf.OneOf<Chatify.Application.ChatGroups.Commands.UserIsNotMemberError,
        System.Collections.Generic.List<Chatify.Domain.Entities.User>>;

namespace Chatify.Web.FastEndpoints_Features.ChatGroups;

[HttpGet("{groupId:guid}/members/search")]
public sealed class SearchMembersEndpoint : BaseChatGroupsEndpoint<EmptyRequest, IResult>
{
    public override async Task<IResult> HandleAsync(EmptyRequest req,
        CancellationToken ct)
    {
        var usernameQuery = Query<string>("q");
        var groupId = Route<Guid>("groupId");

        return await QueryAsync<SearchChatGroupMembersByName, SearchChatGroupMembersByNameResult>(
                new SearchChatGroupMembersByName(groupId, usernameQuery), ct)
            .MatchAsync(
                err => err.ToBadRequestResult(),
                Ok);
    }
}
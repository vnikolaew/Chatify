using Chatify.Application.ChatGroups.Queries;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Extensions;
using FastEndpoints;
using SearchChatGroupsByNameResult =
    OneOf.OneOf<Chatify.Application.Common.Models.BaseError,
        System.Collections.Generic.List<Chatify.Domain.Entities.ChatGroup>>;

namespace Chatify.Web.FastEndpoints_Features.ChatGroups;

[HttpGet("search")]
public sealed class SearchEndpoint : BaseChatGroupsEndpoint<EmptyRequest, IResult>
{
    public override async Task<IResult> HandleAsync(
        EmptyRequest req,
        CancellationToken ct)
    {
        var nameQuery = Query<string>("q");

        return await QueryAsync<SearchChatGroupsByName, SearchChatGroupsByNameResult>(
                new SearchChatGroupsByName(nameQuery), ct)
            .MatchAsync(
                err => err.ToBadRequestResult(),
                Ok);
    }
}
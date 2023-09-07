using Chatify.Application.ChatGroups.Queries;
using Chatify.Web.Extensions;
using FastEndpoints;
using GetChatGroupsFeedResult =
    OneOf.OneOf<Chatify.Application.Common.Models.BaseError,
        System.Collections.Generic.List<Chatify.Application.ChatGroups.Queries.ChatGroupFeedEntry>>;

namespace Chatify.Web.FastEndpoints_Features.ChatGroups;

[HttpGet("feed")]
public sealed class FeedEndpoint : BaseChatGroupsEndpoint<EmptyRequest, IResult>
{
    public override async Task<IResult> HandleAsync(EmptyRequest req,
        CancellationToken ct)
    {
        var limit = Query<int>("limit");
        var offset = Query<int>("offset");

        var result = await QueryAsync<GetChatGroupsFeed, GetChatGroupsFeedResult>(
            new GetChatGroupsFeed(limit, offset), ct);
        return result.Match(
            err => err.ToBadRequestResult(),
            Ok);
    }
}
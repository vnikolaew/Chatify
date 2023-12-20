using Chatify.Application.ChatGroups.Contracts;
using Chatify.Application.Common;
using Chatify.Application.Common.Behaviours.Timing;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using GetChatGroupsFeedResult =
    OneOf.OneOf<Chatify.Application.Common.Models.BaseError,
        System.Collections.Generic.List<Chatify.Application.ChatGroups.Queries.ChatGroupFeedEntry>>;

namespace Chatify.Application.ChatGroups.Queries;

[Timed]
public record GetStarredChatGroupsFeed : IQuery<GetChatGroupsFeedResult>;

internal sealed class GetStarredChatGroupsFeedHandler(IIdentityContext identityContext, IChatGroupsFeedService feedService)
    : BaseQueryHandler<GetStarredChatGroupsFeed, GetChatGroupsFeedResult>(identityContext)
{
    public override async Task<GetChatGroupsFeedResult> HandleAsync(GetStarredChatGroupsFeed command, CancellationToken cancellationToken = default)
        => await feedService.GetStarredFeedEntriesForUserAsync(identityContext.Id, cancellationToken);
}
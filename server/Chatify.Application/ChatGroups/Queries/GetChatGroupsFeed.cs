using System.Text;
using Chatify.Application.ChatGroups.Contracts;
using Chatify.Application.Common.Behaviours.Timing;
using Chatify.Domain.Entities;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.ChatGroups.Queries;

using GetChatGroupsFeedResult = OneOf<Error, List<ChatGroupFeedEntry>>;

public record ChatGroupFeedEntry
{
    public ChatGroup ChatGroup { get; set; }
    
    public Domain.Entities.User User { get; set; }
    
    public ChatMessage ChatMessage { get; set; }
}

[Timed]
public record GetChatGroupsFeed
    (int Limit, int Offset) : IQuery<GetChatGroupsFeedResult>;

internal sealed class GetChatGroupsFeedHandler
    : IQueryHandler<GetChatGroupsFeed, GetChatGroupsFeedResult>
{
    private readonly IIdentityContext _identityContext;
    private readonly IChatGroupsFeedService _feedService;

    public GetChatGroupsFeedHandler(
        IIdentityContext identityContext,
        IChatGroupsFeedService feedService)
    {
        _identityContext = identityContext;
        _feedService = feedService;
    }

    public async Task<GetChatGroupsFeedResult> HandleAsync(
        GetChatGroupsFeed query, CancellationToken cancellationToken = default)
        => await _feedService.GetFeedEntriesForUserAsync(
            _identityContext.Id,
            query.Limit,
            query.Offset,
            cancellationToken);
}
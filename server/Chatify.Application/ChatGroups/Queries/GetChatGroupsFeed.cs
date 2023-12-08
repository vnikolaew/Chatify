using System.Text;
using Chatify.Application.ChatGroups.Contracts;
using Chatify.Application.Common.Behaviours.Timing;
using Chatify.Application.Common.Models;
using Chatify.Domain.Entities;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using OneOf;

namespace Chatify.Application.ChatGroups.Queries;

using GetChatGroupsFeedResult = OneOf<BaseError, List<ChatGroupFeedEntry>>;

public record ChatGroupFeedEntry(
    ChatGroup ChatGroup,
    ChatMessage LatestMessage,
    Domain.Entities.User? MessageSender = default!)
{
    public ChatGroupFeedEntry() : this(new ChatGroup(), new ChatMessage())
    {
    }
}

[Timed]
public record GetChatGroupsFeed
    (int Limit, int Offset) : IQuery<GetChatGroupsFeedResult>;

internal sealed class GetChatGroupsFeedHandler(IIdentityContext identityContext,
        IChatGroupsFeedService feedService)
    : IQueryHandler<GetChatGroupsFeed, GetChatGroupsFeedResult>
{
    public async Task<GetChatGroupsFeedResult> HandleAsync(
        GetChatGroupsFeed query, CancellationToken cancellationToken = default)
        => await feedService.GetFeedEntriesForUserAsync(
            identityContext.Id,
            query.Limit,
            query.Offset,
            cancellationToken);
}
using Chatify.Application.ChatGroups.Queries;

namespace Chatify.Application.ChatGroups.Contracts;

public interface IChatGroupsFeedService
{
    Task<List<ChatGroupFeedEntry>> GetFeedEntriesForUserAsync(
        Guid userId,
        int limit,
        int offset,
        CancellationToken cancellationToken = default);
    
    Task<List<ChatGroupFeedEntry>> GetStarredFeedEntriesForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
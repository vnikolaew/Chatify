using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Shared.Abstractions.Queries;

namespace Chatify.Domain.Repositories;

public interface INotificationRepository : IDomainRepository<UserNotification, Guid>
{
    Task<CursorPaged<UserNotification>> GetPaginatedForUserAsync(
        Guid userId,
        int pageSize,
        string? pagingCursor,
        CancellationToken cancellationToken = default);
    
    Task<List<UserNotification>> AllForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
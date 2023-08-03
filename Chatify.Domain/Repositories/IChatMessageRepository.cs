using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Shared.Abstractions.Queries;

namespace Chatify.Domain.Repositories;

public interface IChatMessageRepository : IDomainRepository<ChatMessage, Guid>
{
    Task<CursorPaged<ChatMessage>> GetPaginatedByGroupAsync(
        Guid groupId,
        int pageSize,
        string pagingCursor,
        CancellationToken cancellationToken = default);
}
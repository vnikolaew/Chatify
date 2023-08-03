using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Shared.Abstractions.Queries;

namespace Chatify.Domain.Repositories;

public interface IChatMessageReplyRepository : IDomainRepository<ChatMessageReply, Guid>
{
    Task<bool> DeleteAllForMessage(Guid messageId, CancellationToken cancellationToken = default);
    
    Task<CursorPaged<ChatMessageReply>> GetPaginatedByMessageAsync(
        Guid messageId,
        int pageSize,
        string pagingCursor,
        CancellationToken cancellationToken);
}
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
   
   Task<CursorPaged<MessageRepliersInfo>> GetPaginatedReplierInfosByGroupAsync(
      Guid groupId,
      int pageSize,
      string pagingCursor,
      CancellationToken cancellationToken = default);

   Task<IDictionary<Guid, ChatMessage>> GetLatestForGroups(
      IEnumerable<Guid> groupIds,
      CancellationToken cancellationToken = default);
   
    Task<List<ChatMessage>?> GetByIds(
        IEnumerable<Guid> messageIds,
        CancellationToken cancellationToken = default);
}
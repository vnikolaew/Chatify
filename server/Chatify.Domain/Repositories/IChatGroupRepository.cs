using Chatify.Domain.Common;
using Chatify.Domain.Entities;

namespace Chatify.Domain.Repositories;

public interface IChatGroupRepository : IDomainRepository<ChatGroup, Guid>
{
    Task<List<ChatGroup>> GetByIds(
        IEnumerable<Guid> groupIds,
        CancellationToken cancellationToken = default);
}
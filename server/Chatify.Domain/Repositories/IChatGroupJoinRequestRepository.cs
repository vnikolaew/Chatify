using Chatify.Domain.Common;
using Chatify.Domain.Entities;

namespace Chatify.Domain.Repositories;

public interface IChatGroupJoinRequestRepository : IDomainRepository<ChatGroupJoinRequest, Guid>
{
    Task<List<ChatGroupJoinRequest>> ByGroup(Guid groupId, CancellationToken cancellationToken = default);
}
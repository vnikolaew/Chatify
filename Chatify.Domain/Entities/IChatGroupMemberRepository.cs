using Chatify.Domain.Common;

namespace Chatify.Domain.Entities;

public interface IChatGroupMemberRepository : IDomainRepository<ChatGroupMember, Guid>
{
    Task<ChatGroupMember?> ByGroupAndUser(
        Guid groupId,
        Guid userId,
        CancellationToken cancellationToken = default);
    
    Task<bool> Exists(
        Guid groupId,
        Guid userId,
        CancellationToken cancellationToken = default);
}
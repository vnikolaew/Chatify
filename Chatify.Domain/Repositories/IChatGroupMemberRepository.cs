using Chatify.Domain.Common;
using Chatify.Domain.Entities;

namespace Chatify.Domain.Repositories;

public interface IChatGroupMemberRepository : IDomainRepository<ChatGroupMember, Guid>
{
    Task<ChatGroupMember?> ByGroupAndUser(
        Guid groupId,
        Guid userId,
        CancellationToken cancellationToken = default);
    
    Task<List<ChatGroupMember>?> ByGroup(
        Guid groupId,
        CancellationToken cancellationToken = default);
    
    Task<List<Guid>> GroupsIdsByUser(
        Guid userId,
        CancellationToken cancellationToken = default);
    
    Task<bool> Exists(
        Guid groupId,
        Guid userId,
        CancellationToken cancellationToken = default);
}
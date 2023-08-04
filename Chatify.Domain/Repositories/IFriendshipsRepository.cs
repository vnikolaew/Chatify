using Chatify.Domain.Common;
using Chatify.Domain.Entities;

namespace Chatify.Domain.Repositories;

public interface IFriendshipsRepository : IDomainRepository<FriendsRelation, Guid>
{
    Task<List<User>> AllForUser(Guid userId, CancellationToken cancellationToken = default);
    
    Task<bool> DeleteForUsers(Guid friendOneId, Guid friendTwoId, CancellationToken cancellationToken = default);

    Task<List<Guid>> AllFriendIdsForUser(
        Guid userId,
        CancellationToken cancellationToken = default);
}
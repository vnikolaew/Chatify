using Chatify.Domain.Common;
using Chatify.Domain.Entities;

namespace Chatify.Domain.Repositories;

public interface IFriendInvitationRepository : IDomainRepository<FriendInvitation, Guid>
{
    Task<List<FriendInvitation>> AllSentByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
    
    Task<List<FriendInvitation>> AllSentToUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
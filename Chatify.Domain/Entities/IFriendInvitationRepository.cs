using Chatify.Domain.Common;

namespace Chatify.Domain.Entities;

public interface IFriendInvitationRepository : IDomainRepository<FriendInvitation, Guid>
{
    Task<List<FriendInvitation>> AllSentByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
    
    Task<List<FriendInvitation>> AllSentToUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
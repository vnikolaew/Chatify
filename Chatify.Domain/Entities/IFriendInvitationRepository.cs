using Chatify.Domain.Common;

namespace Chatify.Domain.Entities;

public interface IFriendInvitationRepository : IDomainRepository<FriendInvitation, Guid>
{
    Task<List<FriendInvitation>> AllForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
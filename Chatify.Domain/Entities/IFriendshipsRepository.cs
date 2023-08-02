using Chatify.Domain.Common;

namespace Chatify.Domain.Entities;

public interface IFriendshipsRepository : IDomainRepository<FriendsRelation, Guid>
{
   Task<List<FriendsRelation>> AllForUser(Guid userId);
}
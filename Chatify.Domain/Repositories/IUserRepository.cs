using Chatify.Domain.Common;
using Chatify.Domain.Entities;

namespace Chatify.Domain.Repositories;

public interface IUserRepository : IDomainRepository<User, Guid>
{
    Task<User?> GetByUsername(
        string usernameQuery,
        CancellationToken cancellationToken = default);
    
    Task<List<User>?> AllByUserIds(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default);
}
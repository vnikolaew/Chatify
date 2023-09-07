using Chatify.Domain.Common;
using Chatify.Domain.Entities;

namespace Chatify.Domain.Repositories;

public interface IUserRepository : IDomainRepository<User, Guid>
{
    Task<List<User>?> SearchByUsername(
        string usernameQuery,
        CancellationToken cancellationToken = default);
    
    Task<List<User>?> GetByIds(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default);
    
    Task<User?> FindByUserHandle(
        string handle,
        CancellationToken cancellationToken = default);

    Task<List<User>?> GetAllWithUsername(
        string username,
        CancellationToken cancellationToken = default);
}
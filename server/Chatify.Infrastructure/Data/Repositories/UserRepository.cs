using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Mappings;
using Chatify.Infrastructure.Data.Models;
using Chatify.Infrastructure.Data.Services;
using Redis.OM;
using Redis.OM.Contracts;
using Redis.OM.Searching;
using Guid = System.Guid;
using IMapper = AutoMapper.IMapper;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class UserRepository(
        IMapper mapper,
        Mapper dbMapper,
        IEntityChangeTracker changeTracker,
        IRedisConnectionProvider connectionProvider)
    : BaseCassandraRepository<Domain.Entities.User, ChatifyUser, Guid>(mapper, dbMapper, changeTracker),
        IUserRepository
{
    private readonly IRedisCollection<ChatifyUser> _cacheUsers
        = connectionProvider.RedisCollection<ChatifyUser>();
        
    public async Task<List<Domain.Entities.User>?> SearchByUsername(
        string usernameQuery,
        CancellationToken cancellationToken = default)
    {
        var users = await _cacheUsers
            .Where(u => u.UserName == usernameQuery)
            .ToListAsync();
        return users
            .To<Domain.Entities.User>(Mapper)
            .ToList();
    }


    public async Task<List<Domain.Entities.User>?> GetByIds(
        IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var usersById = await _cacheUsers
            .FindByIdsAsync(userIds.Select(_ => _.ToString()));
            
        return usersById
            .Values
            .To<Domain.Entities.User>(Mapper)
            .ToList();
    }
}
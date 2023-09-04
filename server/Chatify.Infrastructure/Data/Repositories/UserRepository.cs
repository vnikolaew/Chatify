using Cassandra.Mapping;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Mappings;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Chatify.Infrastructure.Data.Services;
using Redis.OM;
using Redis.OM.Contracts;
using Redis.OM.Searching;
using SmartFormat.Utilities;
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
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        var usersById = await _cacheUsers
            .FindByIdsAsync(userIds.Select(_ => _.ToString()));

        // It's possible some users are not found from cache, so fetch them from DB:
        Guid[] missingUserIds = usersById
            .Where(_ => _.Value is null)
            .Select(_ => Guid.TryParse(_.Key, out var id) ? id : default)
            .ToArray();
        
        if ( missingUserIds.Any() )
        {
            var missingUsers = ( await DbMapper.FetchListAsync<ChatifyUser>(
                    new Cql($"WHERE id IN ({string.Join(", ", missingUserIds.Select(_ => "?"))}) ALLOW FILTERING;")
                        .WithArguments(missingUserIds.Cast<object>().ToArray()))
                ).ToDictionary(_ => _.Id, _ => _);

            foreach ( var id in missingUserIds )
            {
                usersById[id.ToString()] = missingUsers[id];
            }
        }

        return usersById
            .Values
            .To<Domain.Entities.User>(Mapper)
            .ToList();
    }
}
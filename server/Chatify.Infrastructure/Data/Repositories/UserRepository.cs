﻿using Cassandra.Mapping;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Mappings;
using Chatify.Infrastructure.Data.Extensions;
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

    public async Task<Domain.Entities.User?> FindByUserHandle(
        string handle,
        CancellationToken cancellationToken = default)
    {
        var user = await DbMapper.FirstOrDefaultAsync<ChatifyUser>(
            "SELECT * FROM users WHERE user_handle = ? ALLOW FILTERING;",
            handle);
        return user.To<Domain.Entities.User>(Mapper);
    }

    public async Task<List<Domain.Entities.User>?> GetAllWithUsername(
        string username,
        CancellationToken cancellationToken = default)
        => await DbMapper.FetchListAsync<ChatifyUser>(
                "SELECT * FROM users_by_username WHERE normalizedusername = ?;",
                username.ToUpper())
            .ToAsyncList<ChatifyUser, Domain.Entities.User>(Mapper);


    public async Task<List<Domain.Entities.User>?> GetByIds(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        var usersById = await _cacheUsers
            .FindByIdsAsync(userIds.Select(_ => _.ToString()));

        // It's possible some users are not found from cache, so fetch them from DB:
        var missingUserIds = usersById
            .Where(_ => _.Value is null)
            .Select(_ => Guid.TryParse(_.Key, out var id) ? id : default)
            .ToArray();

        if ( missingUserIds.Any() )
        {
            var cqlQuery = $"WHERE id IN ({string.Join(", ", missingUserIds.Select(_ => "?"))}) ALLOW FILTERING;";
            var cql = new Cql(cqlQuery).WithArguments(missingUserIds.Cast<object>().ToArray());

            var missingUsers = ( await DbMapper.FetchListAsync<ChatifyUser>(cql) )
                .ToDictionary(_ => _.Id, _ => _);

            foreach ( var id in missingUserIds )
            {
                if ( missingUsers.TryGetValue(id, out var missingUser) )
                {
                    usersById[id.ToString()] = missingUser;
                }
            }
        }

        return usersById
            .Values
            .Where(_ => _ is not null)
            .To<Domain.Entities.User>(Mapper)
            .ToList();
    }
}
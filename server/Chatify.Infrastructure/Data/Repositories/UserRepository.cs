using System.Reflection;
using Cassandra.Mapping;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Caching.Extensions;
using Chatify.Infrastructure.Common.Mappings;
using Chatify.Infrastructure.Data.Models;
using StackExchange.Redis;
using Guid = System.Guid;
using IMapper = AutoMapper.IMapper;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class UserRepository
    : BaseCassandraRepository<Domain.Entities.User, ChatifyUser, Guid>, IUserRepository
{
    private readonly IDatabase _cache;

    public UserRepository(IMapper mapper, Mapper dbMapper, IDatabase cache)
        : base(mapper, dbMapper) => _cache = cache;

    public Task<Domain.Entities.User?> GetByUsername(
        string usernameQuery,
        CancellationToken cancellationToken = default)
        => DbMapper
            .FirstOrDefaultAsync<ChatifyUser>(
                "SELECT * FROM users_by_username WHERE normalizedusername = ?",
                usernameQuery.ToUpperInvariant())
            .ToAsyncNullable<ChatifyUser, Domain.Entities.User>(Mapper);


    public async Task<List<Domain.Entities.User>?> GetByIds(
        IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var cacheUsers = (await _cache.GetAsync<ChatifyUser>(userIds.Select(id => $"user:{id.ToString()}"))).ToList();
        
        var missingUserIds = userIds.ToHashSet();
        missingUserIds.ExceptWith(cacheUsers.Select(_ => _.Id));
        
        if ( missingUserIds.Any() )
        {
            var paramPlaceholders = string.Join(", ", missingUserIds.Select(_ => "?"));
            var cql = new Cql($"SELECT * FROM users WHERE id IN ({paramPlaceholders});");
            cql.GetType()
                .GetProperty(nameof(Cql.Arguments),
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?
                .SetValue(cql, missingUserIds.ToArray());

            var missingUsers = await DbMapper.FetchAsync<ChatifyUser>(cql);
            cacheUsers.AddRange(missingUsers.ToList());
        }

        return cacheUsers
            .To<Domain.Entities.User>(Mapper)
            .ToList();
    }
}
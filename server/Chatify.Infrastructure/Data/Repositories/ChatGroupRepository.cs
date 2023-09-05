using Cassandra.Mapping;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Mappings;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Chatify.Infrastructure.Data.Services;
using Redis.OM;
using Redis.OM.Contracts;
using Redis.OM.Searching;
using ChatGroup = Chatify.Domain.Entities.ChatGroup;
using IMapper = AutoMapper.IMapper;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class ChatGroupRepository(
        IMapper mapper,
        IRedisConnectionProvider connectionProvider,
        Mapper dbMapper,
        IEntityChangeTracker changeTracker)
    : BaseCassandraRepository<ChatGroup, Models.ChatGroup, Guid>(mapper, dbMapper, changeTracker),
        IChatGroupRepository
{
    private readonly IRedisCollection<Models.ChatGroup> _cacheGroups =
        connectionProvider.RedisCollection<Models.ChatGroup>();

    public async Task<List<ChatGroup>> GetByIds(
        IEnumerable<Guid> groupIds,
        CancellationToken cancellationToken = default)
    {
        var groups = await _cacheGroups
                .FindByIdsAsync(groupIds.Select(_ => _.ToString()));
        
        // It's possible some users are not found from cache, so fetch them from DB:
        Guid[] missingGroupIds = groups
            .Where(_ => _.Value is null)
            .Select(_ => Guid.TryParse(_.Key, out var id) ? id : default)
            .ToArray();
        
        if ( missingGroupIds.Any() )
        {
            var missingGroups = ( await DbMapper.FetchListAsync<Models.ChatGroup>(
                    new Cql($"WHERE id IN ({string.Join(", ", missingGroupIds.Select(_ => "?"))}) ALLOW FILTERING;")
                        .WithArguments(missingGroupIds.Cast<object>().ToArray()))
                ).ToDictionary(_ => _.Id, _ => _);

            foreach ( var id in missingGroupIds )
            {
                groups[id.ToString()] = missingGroups.TryGetValue(id, out var group) ? group : default;
            }
        }

        return groups
            .Values
            .Where(_ => _ is not null)
            .ToList<ChatGroup>(Mapper);
    }

    public async Task<List<ChatGroup>> SearchByName(
        string nameQuery,
        CancellationToken cancellationToken = default)
        => ( await _cacheGroups
                .Where(g => g.Name == nameQuery)
                .ToListAsync() )
            .ToList<ChatGroup>(Mapper);
}
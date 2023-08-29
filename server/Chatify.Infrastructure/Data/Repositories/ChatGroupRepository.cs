using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Mappings;
using Chatify.Infrastructure.Data.Services;
using Redis.OM;
using Redis.OM.Contracts;
using Redis.OM.Searching;
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
        => ( await _cacheGroups
            .FindByIdsAsync(groupIds.Select(_ => _.ToString())) )
        .Values
        .Where(_ => _ is not null)
        .ToList<ChatGroup>(Mapper);

    public async Task<List<ChatGroup>> SearchByName(
        string nameQuery, CancellationToken cancellationToken = default)
        => ( await _cacheGroups
                .Where(g => g.Name == nameQuery)
                .ToListAsync() )
            .ToList<ChatGroup>(Mapper);
}
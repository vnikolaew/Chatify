using System.Reflection;
using Cassandra.Mapping;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Mappings;
using Chatify.Infrastructure.Data.Services;
using IMapper = AutoMapper.IMapper;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class ChatGroupRepository(IMapper mapper, Mapper dbMapper, IEntityChangeTracker changeTracker)
    : BaseCassandraRepository<ChatGroup, Models.ChatGroup, Guid>(mapper, dbMapper, changeTracker),
        IChatGroupRepository
{
    public async Task<List<ChatGroup>> GetByIds(
        IEnumerable<Guid> groupIds,
        CancellationToken cancellationToken = default)
    {
        var paramPlaceholders = string.Join(", ", groupIds.Select(_ => "?"));
        var cql = new Cql($" WHERE id IN ({paramPlaceholders})");

        cql.GetType()
            .GetProperty(nameof(Cql.Arguments),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?
            .SetValue(cql, groupIds.Cast<object>().ToArray());

        return ( await DbMapper
                .FetchAsync<Models.ChatGroup>(cql) )
            .To<ChatGroup>(Mapper)
            .ToList();
    }
}
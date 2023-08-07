using System.Reflection;
using Cassandra.Mapping;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using IMapper = AutoMapper.IMapper;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class ChatGroupRepository
    : BaseCassandraRepository<ChatGroup, Models.ChatGroup, Guid>,
        IChatGroupRepository
{
    public ChatGroupRepository(IMapper mapper, Mapper dbMapper)
        : base(mapper, dbMapper)
    {
    }

    public async Task<List<ChatGroup>> GetByIds(
        IEnumerable<Guid> groupIds,
        CancellationToken cancellationToken = default)
    {
        var paramPlaceholders = string.Join(", ", groupIds.Select(_ => "?"));
        var cql = new Cql($" WHERE id IN ({paramPlaceholders})");

        cql.GetType()
            .GetProperty(nameof(Cql.Arguments),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?
            .SetValue(cql, groupIds);

        var groups = await DbMapper
            .FetchAsync<Models.ChatGroup>(cql);

        return Mapper.Map<List<ChatGroup>>(
            groups ?? new List<Models.ChatGroup>());
    }
}
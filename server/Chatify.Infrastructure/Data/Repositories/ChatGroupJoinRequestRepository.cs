using AutoMapper;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Mappings;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

internal sealed class ChatGroupJoinRequestRepository
    : BaseCassandraRepository<ChatGroupJoinRequest, Models.ChatGroupJoinRequest, Guid>,
        IChatGroupJoinRequestRepository
{
    public ChatGroupJoinRequestRepository(
        IMapper mapper,
        Mapper dbMapper,
        string? idColumn = default)
        : base(mapper, dbMapper, idColumn)
    {
    }

    public async Task<List<ChatGroupJoinRequest>> ByGroup(
        Guid groupId,
        CancellationToken cancellationToken = default)
    {
        var requests = await DbMapper
            .FetchAsync<Models.ChatGroupJoinRequest>(" WHERE chat_group_id = ?", groupId);
        
        return requests.To<ChatGroupJoinRequest>(Mapper)
            .ToList();
    }
}
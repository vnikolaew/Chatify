using AutoMapper;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Mappings;
using Chatify.Infrastructure.Data.Services;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

internal sealed class ChatGroupJoinRequestRepository(
    IMapper mapper,
    Mapper dbMapper,
    IEntityChangeTracker changeTracker,
    string? idColumn = default)
    : BaseCassandraRepository<ChatGroupJoinRequest, Models.ChatGroupJoinRequest, Guid>(mapper, dbMapper, changeTracker,
            idColumn),
        IChatGroupJoinRequestRepository
{
    public async Task<List<ChatGroupJoinRequest>> ByGroup(
        Guid groupId,
        CancellationToken cancellationToken = default)
    {
        const string filter = " WHERE chat_group_id = ?";

        return ( await DbMapper
                .FetchAsync<Models.ChatGroupJoinRequest>(filter, groupId) )
            .To<ChatGroupJoinRequest>(Mapper)
            .ToList();
    }
}
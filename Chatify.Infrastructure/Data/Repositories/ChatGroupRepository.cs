using AutoMapper;
using Chatify.Domain.Entities;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class ChatGroupRepository : BaseCassandraRepository<ChatGroup, Models.ChatGroup, Guid>
{
    public ChatGroupRepository(IMapper mapper, Mapper dbMapper) : base(mapper, dbMapper)
    {
    }
}
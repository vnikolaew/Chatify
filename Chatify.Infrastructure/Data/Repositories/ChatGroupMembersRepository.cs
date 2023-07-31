using AutoMapper;
using Chatify.Infrastructure.Data.Models;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public class ChatGroupMembersRepository : BaseCassandraRepository<ChatGroupMember, Domain.Entities.ChatGroupMember, Guid>
{
    public ChatGroupMembersRepository(IMapper mapper, Mapper dbMapper) : base(mapper, dbMapper)
    {
    }
}
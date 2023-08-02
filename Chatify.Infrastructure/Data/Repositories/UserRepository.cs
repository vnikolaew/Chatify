using Chatify.Domain.Entities;
using Chatify.Infrastructure.Data.Models;
using Guid = System.Guid;
using IMapper = AutoMapper.IMapper;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class UserRepository : BaseCassandraRepository<Domain.Entities.User, ChatifyUser, Guid>
{
    public UserRepository(IMapper mapper, Mapper dbMapper)
        : base(mapper, dbMapper)
    {
    }
}
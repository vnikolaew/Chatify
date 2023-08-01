using AutoMapper;
using Chatify.Domain.Entities;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class FriendshipsRepository : BaseCassandraRepository<FriendsRelation, Models.FriendsRelation, Guid>
{
    public FriendshipsRepository(IMapper mapper, Mapper dbMapper)
        : base(mapper, dbMapper)
    {
    }
}
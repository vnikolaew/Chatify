using AutoMapper;
using Chatify.Domain.Entities;
using Humanizer;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class FriendshipsRepository : BaseCassandraRepository<FriendsRelation, Models.FriendsRelation, Guid>
{
    public FriendshipsRepository(IMapper mapper, Mapper dbMapper)
        : base(mapper, dbMapper, nameof(FriendsRelation.FriendOneId).Underscore())
    {
    }
}
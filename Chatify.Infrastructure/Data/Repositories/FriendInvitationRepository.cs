using AutoMapper;
using Chatify.Domain.Entities;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class FriendInvitationRepository : BaseCassandraRepository<FriendInvitation, Models.FriendInvitation, Guid>
{
    public FriendInvitationRepository(IMapper mapper, Mapper dbMapper) : base(mapper, dbMapper)
    {
    }
}
using AutoMapper;
using Chatify.Domain.Entities;
using Humanizer;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class FriendshipsRepository
    : BaseCassandraRepository<FriendsRelation, Models.FriendsRelation, Guid>,
        IFriendshipsRepository
{
    public FriendshipsRepository(IMapper mapper, Mapper dbMapper)
        : base(mapper, dbMapper, nameof(FriendsRelation.Id).Underscore())
    {
    }

    public async Task<List<FriendsRelation>> AllForUser(Guid userId)
    {
        var relations = await DbMapper
            .FetchAsync<Models.FriendsRelation>("WHERE friend_one_id = ?", userId);
        return Mapper.Map<List<FriendsRelation>>(relations);
    }
}
using AutoMapper;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Data.Models;
using Chatify.Shared.Abstractions.Serialization;
using Humanizer;
using StackExchange.Redis;
using FriendsRelation = Chatify.Domain.Entities.FriendsRelation;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class FriendshipsRepository
    : BaseCassandraRepository<FriendsRelation, Models.FriendsRelation, Guid>,
        IFriendshipsRepository
{
    private readonly IDatabase _cache;
    private readonly ISerializer _serializer;

    public FriendshipsRepository(IMapper mapper, Mapper dbMapper, IDatabase cache, ISerializer serializer)
        : base(mapper, dbMapper, nameof(FriendsRelation.Id).Underscore())
    {
        _cache = cache;
        _serializer = serializer;
    }

    public async Task<List<FriendsRelation>> AllForUser(Guid userId)
    {
        var relations = await DbMapper
            .FetchAsync<Models.FriendsRelation>("WHERE friend_one_id = ?", userId);
        return Mapper.Map<List<FriendsRelation>>(relations);
    }

    public async Task<List<Guid>> AllFriendIdsForUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var friendIds = await _cache.SortedSetRangeByScoreAsync($"user:{userId}:friends", order: Order.Descending);
        return friendIds.Select(id => Guid.Parse(id.ToString())).ToList();
    }

    public async Task<List<Domain.Entities.User>> AllForUser(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var friendIds = await _cache.SortedSetRangeByScoreAsync($"user:{userId}:friends", order: Order.Descending);

        List<ChatifyUser> users = new List<ChatifyUser>();
        
        // Chunk Ids to ease work for cache server processing:
        foreach (var idsChunk in friendIds.Chunk(10))
        {
            var friendsStrings =
                await _cache.StringGetAsync(
                    idsChunk.Select(id => new RedisKey($"user:{id.ToString()}")).ToArray());
            
            users.AddRange(
                friendsStrings.Select(f => _serializer.Deserialize<ChatifyUser>(f.ToString()))!);
        }

        return users.Any()
            ? Mapper.Map<List<Domain.Entities.User>>(users)
            : new List<Domain.Entities.User>();
    }
}
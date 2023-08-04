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

    public FriendshipsRepository(
        IMapper mapper, Mapper dbMapper,
        IDatabase cache, ISerializer serializer)
        : base(mapper, dbMapper, nameof(FriendsRelation.Id).Underscore())
    {
        _cache = cache;
        _serializer = serializer;
    }

    public new async Task<FriendsRelation> SaveAsync(
        FriendsRelation entity,
        CancellationToken cancellationToken = default)
    {
        var result = await base.SaveAsync(entity, cancellationToken);

        var userOne = DbMapper
            .FirstOrDefaultAsync<ChatifyUser>("WHERE id = ?", entity.FriendOneId);

        var userTwo = DbMapper
            .FirstOrDefaultAsync<ChatifyUser>("WHERE id = ?", entity.FriendTwoId);

        var users = await Task.WhenAll(userOne, userTwo);

        // Store each user as a friend in the other user's sorted set of friends:
        var storeTasks = new Task[]
        {
            _cache.SortedSetAddAsync(
                new RedisKey($"user:{entity.FriendOneId}:friends"),
                new RedisValue(entity.FriendTwoId.ToString()),
                entity.CreatedAt.Ticks,
                SortedSetWhen.NotExists),
            _cache.SortedSetAddAsync(
                new RedisKey($"user:{entity.FriendTwoId}:friends"),
                new RedisValue(entity.FriendOneId.ToString()),
                entity.CreatedAt.Ticks,
                SortedSetWhen.NotExists),
            _cache.StringSetAsync(
                new RedisKey($"user:{entity.FriendOneId}"),
                new RedisValue(_serializer.Serialize(users[0]))),
            _cache.StringSetAsync(
                new RedisKey($"user:{entity.FriendTwoId}"),
                new RedisValue(_serializer.Serialize(users[1]))),
        };

        await Task.WhenAll(storeTasks);
        return result;
    }

    public async Task<List<Domain.Entities.User>> AllForUser(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var friendsIds = await _cache.SortedSetRangeByScoreAsync(
            new RedisKey($"user:{userId}:friends"), order: Order.Descending);

        // Potentially chunk Ids to ease cache server processing:
        var friendsStrings = new List<string>();
        foreach (var idsChunk in friendsIds.Chunk(10))
        {
            var redisValues = await _cache.StringGetAsync(
                idsChunk.Select(id => new RedisKey($"user:{id.ToString()}")).ToArray()
            );

            friendsStrings.AddRange(
                redisValues.Select(v => v.ToString())
            );
        }

        return Mapper.Map<List<Domain.Entities.User>>(
            friendsStrings
                .Select(f => _serializer.Deserialize<ChatifyUser>(f.ToString())));
    }

    public async Task<List<Guid>> AllFriendIdsForUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var friendsIds = await _cache.SortedSetRangeByScoreAsync(
            new RedisKey($"user:{userId}:friends"), order: Order.Descending);
        return friendsIds.Select(id => Guid.Parse(id.ToString())).ToList();
    }
}
using AutoMapper;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Caching.Extensions;
using Chatify.Infrastructure.Common.Mappings;
using Chatify.Infrastructure.Data.Models;
using Chatify.Shared.Abstractions.Serialization;
using Humanizer;
using LanguageExt;
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
        : base(mapper, dbMapper, nameof(FriendsRelation.FriendOneId).Underscore())
    {
        _cache = cache;
        _serializer = serializer;
    }

    private static RedisKey GetUserFriendsCacheKey(Guid userId)
        => $"user:{userId}:friends";

    private static RedisKey GetUserCacheKey(Guid userId)
        => $"user:{userId}";

    public new async Task<FriendsRelation> SaveAsync(
        FriendsRelation entity,
        CancellationToken cancellationToken = default)
    {
        // Make a duplicate DB entry with friend Ids swapped (for better querying):
        var swappedFriendRelation = new FriendsRelation
        {
            Id = entity.Id,
            FriendTwoId = entity.FriendOneId,
            FriendOneId = entity.FriendTwoId,
            CreatedAt = entity.CreatedAt
        };

        var saveOne = base.SaveAsync(entity, cancellationToken);
        var saveTwo = base.SaveAsync(swappedFriendRelation, cancellationToken);
        await Task.WhenAll(saveOne, saveTwo);

        // Store each user as a friend in the other user's sorted set of friends:
        var storeTasks = new[]
        {
            _cache.SortedSetAddAsync(
                new RedisKey(GetUserFriendsCacheKey(entity.FriendOneId)),
                new RedisValue(entity.FriendTwoId.ToString()),
                entity.CreatedAt.Ticks,
                SortedSetWhen.NotExists),
            _cache.SortedSetAddAsync(
                new RedisKey(GetUserFriendsCacheKey(entity.FriendTwoId)),
                new RedisValue(entity.FriendOneId.ToString()),
                entity.CreatedAt.Ticks,
                SortedSetWhen.NotExists),
            // _cache.SetAsync($"user:{entity.FriendOneId}", users[0]),
            // _cache.SetAsync($"user:{entity.FriendTwoId}", users[1]),
        };

        var results = await Task.WhenAll(storeTasks);
        return saveOne.Result;
    }

    public async Task<List<Domain.Entities.User>> AllForUser(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var friendsIds = await _cache.SortedSetRangeByScoreAsync(
            GetUserFriendsCacheKey(userId), order: Order.Descending);

        // Potentially chunk Ids to ease cache server processing:
        var friendsStrings = new List<string>();
        foreach ( var idsChunk in friendsIds.Chunk(10) )
        {
            var chunkStrings = await _cache.GetAsync<string>(idsChunk.Select(id => $"user:{id.ToString()}"));
            friendsStrings.AddRange(chunkStrings!);
        }

        return friendsStrings
            .Select(f => _serializer.Deserialize<ChatifyUser>(f))
            .AsQueryable()
            .To<Domain.Entities.User>(Mapper)
            .ToList();
    }

    public async Task<bool> DeleteForUsers(
        Guid friendOneId,
        Guid friendTwoId,
        CancellationToken cancellationToken = default)
    {
        var friendsRelation = await DbMapper
            .FirstOrDefaultAsync<Models.FriendsRelation>(
                " WHERE friend_one_id = ? AND friend_two_id = ? ALLOW FILTERING;",
                friendOneId, friendTwoId);
        if ( friendsRelation is null ) return false;

        await DbMapper.DeleteAsync<Models.FriendsRelation>(
            " WHERE friend_one_id = ? AND friend_two_id = ? ALLOW FILTERING;",
            friendOneId, friendTwoId);

        // Purge entries from cache as well:
        var deleteTasks = new Task[]
        {
            _cache.SortedSetRemoveAsync(
                GetUserFriendsCacheKey(friendOneId),
                new RedisValue(friendTwoId.ToString())),
            _cache.SortedSetRemoveAsync(
                GetUserFriendsCacheKey(friendTwoId),
                new RedisValue(friendOneId.ToString())),
        };

        await Task.WhenAll(deleteTasks);
        return true;
    }

    public async Task<List<Guid>> AllFriendIdsForUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var friendsIds = await _cache.SortedSetRangeByScoreAsync(
            GetUserFriendsCacheKey(userId), order: Order.Descending);

        return friendsIds.Select(id => Guid.Parse(id.ToString())).ToList();
    }

    public async Task<List<FriendsRelation>> AllFriendshipsForUser(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var friendships = await
            DbMapper.FetchAsync<Models.FriendsRelation>(
                " WHERE friend_one_id = ?", userId);

        return friendships.To<FriendsRelation>(Mapper).ToList();
    }
}
using AutoMapper;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Caching.Extensions;
using Chatify.Infrastructure.Common.Mappings;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Services;
using Chatify.Shared.Abstractions.Serialization;
using Humanizer;
using Redis.OM.Contracts;
using StackExchange.Redis;
using FriendsRelation = Chatify.Domain.Entities.FriendsRelation;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class FriendshipsRepository(
        IMapper mapper,
        Mapper dbMapper,
        IEntityChangeTracker changeTracker,
        IDatabase cache,
        IUserRepository users,
        IRedisConnectionProvider connectionProvider)
    : BaseCassandraRepository<FriendsRelation, Models.FriendsRelation, Guid>(mapper, dbMapper,
            changeTracker,
            nameof(FriendsRelation.Id).Underscore()),
        IFriendshipsRepository
{
    public new async Task<FriendsRelation> SaveAsync(
        FriendsRelation entity,
        CancellationToken cancellationToken = default)
    {
        // Make a duplicate DB entry with friend Ids swapped (for better querying):
        var swappedFriendRelation = new FriendsRelation
        {
            GroupId = entity.GroupId,
            Id = entity.Id,
            FriendTwoId = entity.FriendOneId,
            FriendOneId = entity.FriendTwoId,
            CreatedAt = entity.CreatedAt
        };

        var saveOne = base.SaveAsync(entity, cancellationToken);
        var saveTwo = base.SaveAsync(swappedFriendRelation, cancellationToken);
        await Task.WhenAll(saveOne, saveTwo);

        return saveOne.Result;
    }

    public async Task<List<Domain.Entities.User>> AllForUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var friendsIds = await cache.SortedSetRangeByScoreAsync(
            userId.GetUserFriendsKey(), order: Order.Descending);

        // Chunk Ids to ease cache server processing:
        var friends = new List<Domain.Entities.User>();
        foreach ( var idsChunk in friendsIds.Chunk(10) )
        {
            var usersChunk =
                await users.GetByIds(idsChunk
                        .Select(_ => Guid.TryParse(_.ToString(), out var id)
                            ? id : default),
                    cancellationToken);
            friends.AddRange(usersChunk ?? new List<Domain.Entities.User>() );
        }

        return friends;
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
            cache.SortedSetRemoveAsync(
                friendOneId.GetUserFriendsKey(),
                new RedisValue(friendTwoId.ToString())),
            cache.SortedSetRemoveAsync(
                friendTwoId.GetUserFriendsKey(),
                new RedisValue(friendOneId.ToString())),
        };

        await Task.WhenAll(deleteTasks);
        return true;
    }

    public async Task<List<Guid>> AllFriendIdsForUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var friendsIds = await cache.SortedSetRangeByScoreAsync(
            userId.GetUserFriendsKey(), order: Order.Descending);

        return friendsIds.Select(id => Guid.Parse(id.ToString())).ToList();
    }

    public async Task<List<FriendsRelation>> AllFriendshipsForUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var friendships = await DbMapper
            .FetchListAsync<Models.FriendsRelation>(
                " WHERE friend_one_id = ?", userId);

        return friendships
            .To<FriendsRelation>(Mapper)
            .ToList();
    }
}
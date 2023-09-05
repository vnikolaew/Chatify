using Cassandra.Mapping;
using Chatify.Infrastructure.Common.Caching.Extensions;
using Chatify.Infrastructure.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Chatify.Infrastructure.Data.Seeding;

internal sealed class FriendsSeeder(IServiceScopeFactory scopeFactory)
    : BaseSeeder<FriendsRelation>(scopeFactory)
{
    public override int Priority => 2;

    protected override async Task SeedCoreAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = ScopeFactory.CreateAsyncScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var cache = scope.ServiceProvider.GetRequiredService<IDatabase>();

        var insertedMembers = new HashSet<(Guid, Guid)>();

        var users = ( await mapper
            .FetchAsync<ChatifyUser>("SELECT * FROM users;") ).ToArray();

        foreach ( var _ in Enumerable.Range(1, 100) )
        {
            var userOne = users[Random.Shared.Next(0, users.Length)];
            var userTwo = users[Random.Shared.Next(0, users.Length)];

            if ( insertedMembers.Contains(( userOne.Id, userTwo.Id )) ||
                 insertedMembers.Contains(( userTwo.Id, userOne.Id ))
                 || userOne.Id == userTwo.Id ) continue;

            var friendshipOne = new FriendsRelation
            {
                Id = Guid.NewGuid(),
                FriendOneId = userOne.Id,
                FriendTwoId = userTwo.Id,
                CreatedAt = DateTimeOffset.Now,
                GroupId = Guid.NewGuid()
            };

            var friendshipTwo = new FriendsRelation
            {
                Id = Guid.NewGuid(),
                FriendOneId = userTwo.Id,
                FriendTwoId = userOne.Id,
                CreatedAt = friendshipOne.CreatedAt,
                GroupId = friendshipOne.GroupId
            };

            var newGroup = new ChatGroup
            {
                Id = friendshipOne.GroupId,
                CreatedAt = DateTimeOffset.Now,
                Name = $"{friendshipOne.FriendOneId}:{friendshipOne.FriendTwoId}",
                AdminIds = new HashSet<Guid> { friendshipOne.FriendOneId, friendshipOne.FriendTwoId },
                Metadata = new Dictionary<string, string> { { "private", "true" } }
            };

            var memberships = new ChatGroupMember[]
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTimeOffset.Now,
                    MembershipType = 0,
                    ChatGroupId = newGroup.Id,
                    Username = userOne.UserName,
                    UserId = userOne.Id
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTimeOffset.Now,
                    MembershipType = 0,
                    ChatGroupId = newGroup.Id,
                    Username = userTwo.UserName,
                    UserId = userTwo.Id
                }
            };

            var idIndex = Math.Max(0,
                Math.Min(1, Random.Shared.Next(0, 2))
            );

            newGroup.CreatorId = newGroup.AdminIds.ToArray()[idIndex];
            insertedMembers.Add(( userOne.Id, userTwo.Id ));

            await mapper.InsertAsync(friendshipOne, insertNulls: true);
            await mapper.InsertAsync(friendshipTwo, insertNulls: true);
            await Task.WhenAll(memberships.Select(m =>
                mapper.InsertAsync(m, insertNulls: true)));

            await mapper.InsertAsync(newGroup, insertNulls: false);

            // Insert friend Ids in Redis Sorted set caches:
            var cacheSaveTasks = new Task[]
            {
                cache.AddUserFriendAsync(
                    userOne.Id,
                    userTwo.Id,
                    friendshipOne.CreatedAt),

                cache.AddUserFriendAsync(
                    userTwo.Id,
                    userOne.Id,
                    friendshipTwo.CreatedAt),
            };
            await Task.WhenAll(cacheSaveTasks);
        }
    }
}
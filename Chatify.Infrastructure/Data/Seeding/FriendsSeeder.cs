using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Chatify.Infrastructure.Data.Seeding;

internal sealed class FriendsSeeder : ISeeder
{
    public int Priority => 3;
    
    private readonly IServiceScopeFactory _scopeFactory;

    public FriendsSeeder(IServiceScopeFactory scopeFactory)
        => _scopeFactory = scopeFactory;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        
        var insertedMembers = new HashSet<(Guid, Guid)>();
        
        var userIds = (await mapper
            .FetchAsync<Guid>("SELECT id FROM users;")).ToArray();
        
        foreach (var _ in Enumerable.Range(1, 100))
        {
            var userOneId = userIds[Random.Shared.Next(0, userIds.Length)];
            var userTwoId = userIds[Random.Shared.Next(0, userIds.Length)];

            if(insertedMembers.Contains((userOneId, userTwoId))||
               insertedMembers.Contains((userTwoId, userOneId))) continue;

            var friends = new FriendsRelation
            {
                Id = Guid.NewGuid(),
                FriendOneId = userOneId,
                FriendTwoId = userTwoId,
                CreatedAt = DateTimeOffset.Now
            };
            await mapper.InsertAsync(friends, insertNulls: true);
        }
    }
}
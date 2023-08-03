using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Chatify.Infrastructure.Data.Seeding;

internal sealed class ChatGroupMemberSeeder : ISeeder
{
    public int Priority => 2;

    private readonly IServiceScopeFactory _scopeFactory;

    public ChatGroupMemberSeeder(IServiceScopeFactory scopeFactory)
        => _scopeFactory = scopeFactory;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        
        var insertedMembers = new HashSet<(Guid, Guid)>();
            
        var userIds = (await mapper
            .FetchAsync<Guid>("SELECT id FROM users;")).ToArray();
        
        var groupIds = (await mapper
            .FetchAsync<Guid>("SELECT id FROM chat_groups;")).ToArray();
        
        foreach (var _ in Enumerable.Range(1, 100))
        {
            var userId = userIds[Random.Shared.Next(0, userIds.Length)];
            var groupId = groupIds[Random.Shared.Next(0, groupIds.Length)];

            if(insertedMembers.Contains((userId, groupId))) continue;
            var member = new ChatGroupMember
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.Now,
                UserId = userId,
                ChatGroupId = groupId
            };
            insertedMembers.Add((userId, groupId));
            await mapper.InsertAsync(member, insertNulls: true);
        }
    }
}
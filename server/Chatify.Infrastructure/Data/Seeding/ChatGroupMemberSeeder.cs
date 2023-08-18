using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Chatify.Infrastructure.Data.Seeding;

internal sealed class ChatGroupMemberSeeder(IServiceScopeFactory scopeFactory) : ISeeder
{
    public int Priority => 2;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
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
            
            var memberId = Guid.NewGuid();
            var member = new ChatGroupMember
            {
                Id = memberId,
                CreatedAt = DateTimeOffset.Now,
                UserId = userId,
                ChatGroupId = groupId
            };
            insertedMembers.Add((userId, groupId));
            
            await mapper.InsertAsync(member, insertNulls: true);
            await mapper.ExecuteAsync(
                " UPDATE chat_group_members_count SET members_count = members_count + 1 WHERE id = ?", groupId);
        }
    }
}
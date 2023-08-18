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
            
        var users = (await mapper
            .FetchAsync<ChatifyUser>("SELECT * FROM users;")).ToArray();
        
        var groups = (await mapper
            .FetchAsync<ChatGroup>("SELECT * FROM chat_groups;")).ToArray();
        
        foreach (var _ in Enumerable.Range(1, 100))
        {
            var user = users[Random.Shared.Next(0, users.Length)];
            var group = groups[Random.Shared.Next(0, groups.Length)];

            if(insertedMembers.Contains((user.Id, group.Id))) continue;
            
            var memberId = Guid.NewGuid();
            var member = new ChatGroupMember
            {
                Id = memberId,
                Username = user.UserName,
                CreatedAt = DateTimeOffset.Now,
                UserId = user.Id,
                ChatGroupId = group.Id,
            };
            
            insertedMembers.Add((user.Id, group.Id));
            
            await mapper.InsertAsync(member, insertNulls: false);
            var count = await mapper.FirstOrDefaultAsync<long?>(" SELECT members_count FROM chat_group_members WHERE chat_group_id = ?",
                group.Id);

            await mapper.ExecuteAsync("UPDATE chat_group_members SET members_count = ?  WHERE chat_group_id = ?", (count ?? 0) + 1,
                group.Id);
        }
    }
}
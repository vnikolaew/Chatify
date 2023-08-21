using Cassandra.Mapping;
using Chatify.Infrastructure.Common.Caching.Extensions;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Chatify.Infrastructure.Data.Seeding;

internal sealed class ChatGroupMemberSeeder(IServiceScopeFactory scopeFactory)
    : ISeeder
{
    public int Priority => 2;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var cache = scope.ServiceProvider.GetRequiredService<IDatabase>();

        var insertedMembers = new HashSet<(Guid, Guid)>();

        var users = await mapper.FetchListAsync<ChatifyUser>();
        var groups = await mapper.FetchListAsync<ChatGroup>();

        foreach ( var _ in Enumerable.Range(1, 100) )
        {
            var user = users[Random.Shared.Next(0, users.Count)];
            var group = groups[Random.Shared.Next(0, groups.Count)];

            if ( insertedMembers.Contains(( user.Id, group.Id )) ) continue;

            var memberId = Guid.NewGuid();
            var member = new ChatGroupMember
            {
                Id = memberId,
                Username = user.UserName,
                CreatedAt = DateTimeOffset.Now,
                UserId = user.Id,
                ChatGroupId = group.Id,
            };

            insertedMembers.Add(( user.Id, group.Id ));

            await mapper.InsertAsync(member, insertNulls: false);
            var groupKey = GetGroupMembersCacheKey(member.ChatGroupId);
            await cache.BloomFilterAddAsync(groupKey, member.UserId.ToString());

            var count = await mapper.FirstOrDefaultAsync<long?>(
                "SELECT members_count FROM chat_group_members WHERE chat_group_id = ?",
                group.Id);

            await mapper.ExecuteAsync(
                """
                UPDATE chat_group_members SET members_count = ?
                        WHERE chat_group_id = ?
                """, ( count ?? 0 ) + 1,
                group.Id);

            await mapper.ExecuteAsync(
                """
                UPDATE chat_group_members_count
                SET members_count = members_count + 1 WHERE chat_group_id = ?;
                """,
                group.Id);
        }
    }

    private static string GetGroupMembersCacheKey(Guid groupId)
        => $"groups:{groupId.ToString()}:members";
}
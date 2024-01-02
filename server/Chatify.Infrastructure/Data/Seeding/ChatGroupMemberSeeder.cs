using Cassandra.Mapping;
using Chatify.Infrastructure.Common.Caching.Extensions;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Guid = System.Guid;

namespace Chatify.Infrastructure.Data.Seeding;

internal sealed class ChatGroupMemberSeeder(IServiceScopeFactory scopeFactory)
    : BaseSeeder<ChatGroupMember>(scopeFactory)
{
    public override int Priority => 3;

    protected override async Task SeedCoreAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var cache = scope.ServiceProvider.GetRequiredService<IDatabase>();

        var users = await mapper.FetchListAsync<ChatifyUser>();
        var groups = await mapper.FetchListAsync<ChatGroup>();

        foreach ( var group in groups.Where(g =>
                     !g.Metadata.TryGetValue("private", out var isPrivate) && isPrivate != "true") )
        {
            var addedUserIds = new HashSet<Guid>();
            foreach ( var _ in Enumerable.Range(1, 25) )
            {
                var user = PickNewMember(addedUserIds, users);

                var memberId = Guid.NewGuid();
                var member = new ChatGroupMember
                {
                    Id = memberId,
                    Username = user.UserName,
                    CreatedAt = DateTimeOffset.Now,
                    UserId = user.Id,
                    ChatGroupId = group.Id,
                };

                await mapper.InsertAsync(member, insertNulls: false);
                await cache.AddGroupMemberAsync(member.ChatGroupId, member.UserId);

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
    }

    private static ChatifyUser PickNewMember(
        HashSet<Guid> insertedMembers,
        List<ChatifyUser> users)
    {
        while ( true )
        {
            var user = users[Random.Shared.Next(0, users.Count)];
            if ( !insertedMembers.Add(user.Id) ) continue;

            return user;
        }
    }
}
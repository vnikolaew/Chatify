using Bogus;
using Chatify.Infrastructure.Common.Caching.Extensions;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using Redis.OM.Contracts;
using StackExchange.Redis;
using Guid = System.Guid;
using IMapper = Cassandra.Mapping.IMapper;

namespace Chatify.Infrastructure.Data.Seeding;

internal sealed class ChatGroupSeeder(IServiceScopeFactory scopeFactory)
    : BaseSeeder<ChatGroup>(scopeFactory)
{
    public override int Priority => 1;

    private readonly Faker<ChatGroup> _groupsFaker = new Faker<ChatGroup>()
        .RuleFor(g => g.Id, _ => Guid.NewGuid())
        .RuleFor(g => g.About, f => f.Lorem.Sentences(2, " "))
        .RuleFor(g => g.CreatedAt, f => f.Date.Past())
        .RuleFor(g => g.Name, f => f.Company.CompanyName())
        .RuleFor(g => g.Picture, f => new Media { MediaUrl = f.Internet.Avatar(), Id = Guid.NewGuid() });

    protected override async Task SeedCoreAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = ScopeFactory.CreateAsyncScope();

        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var provider = scope.ServiceProvider
            .GetRequiredService<IRedisConnectionProvider>();
        var cache = scope.ServiceProvider.GetRequiredService<IDatabase>();

        var groups = _groupsFaker.Generate(50);

        var users = await mapper.FetchListAsync<ChatifyUser>("SELECT * FROM users;");
        var seenUserIds = new HashSet<Guid>();

        foreach ( var chatGroup in groups )
        {
            var user = PickUnusedUser(users, seenUserIds);

            chatGroup.CreatorId = user.Id;
            chatGroup.AdminIds.Add(chatGroup.CreatorId);

            await mapper.InsertAsync(chatGroup, insertNulls: true);
            await provider.RedisCollection<ChatGroup>().InsertAsync(chatGroup);

            var groupMember = new ChatGroupMember
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.Now,
                UserId = chatGroup.CreatorId,
                Username = user.UserName,
                MembershipType = 0,
                ChatGroupId = chatGroup.Id
            };

            await mapper.InsertAsync(groupMember);
            await cache.AddGroupMemberAsync(chatGroup.Id, user.Id);
        }
    }

    private static ChatifyUser PickUnusedUser(
        List<ChatifyUser> users,
        HashSet<Guid> seenUserIds)
    {
        ChatifyUser user;
        while ( true )
        {
            user = users[Random.Shared.Next(0, users.Count)];
            if ( seenUserIds.Add(user.Id) ) break;
        }

        return user;
    }
}
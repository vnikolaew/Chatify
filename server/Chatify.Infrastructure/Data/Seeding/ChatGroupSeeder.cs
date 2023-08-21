using Bogus;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using Guid = System.Guid;
using IMapper = Cassandra.Mapping.IMapper;

namespace Chatify.Infrastructure.Data.Seeding;

internal sealed class ChatGroupSeeder : ISeeder
{
    public int Priority => 1;

    private readonly Faker<ChatGroup> _groupsFaker;
    private readonly IServiceScopeFactory _scopeFactory;

    public ChatGroupSeeder(IServiceScopeFactory scopeFactory)
    {
        _groupsFaker = new Faker<ChatGroup>()
            .RuleFor(g => g.Id, _ => Guid.NewGuid())
            .RuleFor(g => g.About, f => f.Lorem.Sentences(2, " "))
            .RuleFor(g => g.Name, f => f.Company.CompanyName())
            .RuleFor(g => g.Picture, f => new Media { MediaUrl = f.Internet.Avatar(), Id = Guid.NewGuid() });

        _scopeFactory = scopeFactory;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var groups = _groupsFaker.Generate(20);

        var users = await mapper.FetchListAsync<ChatifyUser>("SELECT * FROM users;");
        var seenUserIds = new HashSet<Guid>();

        foreach ( var chatGroup in groups )
        {
            var user = PickUnusedUser(users, seenUserIds);

            chatGroup.CreatorId = user.Id;
            chatGroup.AdminIds.Add(chatGroup.CreatorId);

            await mapper.InsertAsync(chatGroup, insertNulls: true);
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
            if ( !seenUserIds.Contains(user.Id) )
            {
                seenUserIds.Add(user.Id);
                break;
            }
        }

        return user;
    }
}
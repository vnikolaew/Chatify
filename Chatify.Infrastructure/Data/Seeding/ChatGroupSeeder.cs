using Bogus;
using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using Guid = System.Guid;

namespace Chatify.Infrastructure.Data.Seeding;

internal sealed class ChatGroupSeeder : ISeeder
{
    public int Priority => 1;

    private readonly Faker<ChatGroup> _groupsFaker;
    private readonly IServiceScopeFactory _scopeFactory;

    public ChatGroupSeeder(
        IServiceScopeFactory scopeFactory)
    {
        _groupsFaker = new Faker<ChatGroup>()
            .RuleFor(g => g.Id, _ => Guid.NewGuid())
            .RuleFor(g => g.About, f => f.Lorem.Sentences(2, " "))
            .RuleFor(g => g.Name, f => f.Company.CompanyName(0))
            .RuleFor(g => g.PictureUrl, f => f.Internet.Avatar());
        
        _scopeFactory = scopeFactory;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var groups = _groupsFaker.Generate(20);

        var userIds = (await mapper
            .FetchAsync<Guid>("SELECT id FROM users;")).ToArray();
        
        foreach (var chatGroup in groups)
        {
            chatGroup.CreatorId = userIds[Random.Shared.Next(0, userIds.Length)];
            chatGroup.AdminIds.Add(chatGroup.CreatorId);

            await mapper.InsertAsync(chatGroup, insertNulls: true);
        }
    }
}
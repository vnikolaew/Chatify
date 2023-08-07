using Bogus;
using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Chatify.Infrastructure.Data.Seeding;

internal class ChatGroupMessageSeeder : ISeeder
{
    public int Priority => 4;
    
    protected readonly Faker<ChatMessage> MessageFaker;
    protected readonly IServiceScopeFactory ScopeFactory;

    public ChatGroupMessageSeeder(IServiceScopeFactory scopeFactory)
    {
        ScopeFactory = scopeFactory;
        MessageFaker = new Faker<ChatMessage>()
            .RuleFor(m => m.Id, _ => Guid.NewGuid())
            .RuleFor(m => m.CreatedAt, f => f.Date.Past())
            .RuleFor(m => m.Content, f => f.Lorem.Sentences(2))
            .RuleFor(m => m.ReactionCounts, _ => new Dictionary<int, long>());
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = ScopeFactory.CreateAsyncScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        var members = ( await mapper
                .FetchAsync<ChatGroupMember>("SELECT chat_group_id, user_id FROM chat_group_members;") )
            .ToList();

        var messages = MessageFaker.Generate(100);
        foreach ( var message in messages )
        {
            message.UserId = members[Random.Shared.Next(0, members.Count)].UserId;
            message.ChatGroupId = members[Random.Shared.Next(0, members.Count)].ChatGroupId;

            await mapper.InsertAsync(message, insertNulls: true);
        }
    }
}
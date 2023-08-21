using Bogus;
using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Chatify.Infrastructure.Data.Seeding;

internal sealed class ChatMessageReactionsSeeder : ISeeder
{
    public int Priority => 6;

    private readonly Faker<ChatMessageReaction> _reactionFaker;
    private readonly IServiceScopeFactory _scopeFactory;

    public ChatMessageReactionsSeeder(
        IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _reactionFaker = new Faker<ChatMessageReaction>()
            .RuleFor(r => r.Id, _ => Guid.NewGuid())
            .RuleFor(r => r.CreatedAt, f => f.Date.Past())
            .RuleFor(r => r.ReactionType, f => f.Random.SByte(0));
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        var messages = ( await mapper.FetchAsync<ChatMessage>() ).ToList();
        var groups = ( await mapper.FetchAsync<ChatGroup>() ).ToList();
        var members = ( await mapper.FetchAsync<ChatGroupMember>() ).ToList();

        var reactions = _reactionFaker.Generate(300);

        foreach ( var reaction in reactions )
        {
            var group = groups[Random.Shared.Next(0, groups.Count)];
            var groupMembers = members
                .Where(m => m.ChatGroupId == group.Id)
                .ToList();
            var groupMessages = messages
                .Where(m => m.ChatGroupId == group.Id)
                .ToList();

            if ( group is null || !groupMembers.Any() || !groupMessages.Any() ) continue;

            reaction.ChatGroupId = group.Id;
            reaction.MessageId = groupMessages[Random.Shared.Next(0, groupMessages.Count)].Id;
            reaction.UserId = groupMembers[Random.Shared.Next(0, groupMembers.Count)].Id;

            var reactionCounts = await mapper.FirstOrDefaultAsync<Dictionary<short, long>>(
                "SELECT reaction_counts FROM chat_message_reactions WHERE message_id = ?;",
                reaction.MessageId) ?? new Dictionary<short, long>();

            if ( !reactionCounts.ContainsKey(reaction.ReactionType) ) reactionCounts[reaction.ReactionType] = 0;
            reactionCounts[reaction.ReactionType]++;
            reaction.ReactionCounts = reactionCounts;

            await mapper.InsertAsync(reaction, insertNulls: false);

            // Update Message Reaction Counts:
            var message = groupMessages.FirstOrDefault(m => m.Id == reaction.MessageId)!;
            if ( !message.ReactionCounts.ContainsKey(reaction.ReactionType) )
            {
                message.ReactionCounts[reaction.ReactionType] = 0;
            }

            message.ReactionCounts[reaction.ReactionType]++;

            await mapper.InsertAsync(message);
        }
    }
}
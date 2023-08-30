using Bogus;
using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Chatify.Infrastructure.Data.Seeding;

internal sealed class ChatMessageReactionsSeeder : ISeeder
{
    public int Priority => 6;

    private readonly Faker<ChatMessageReaction> _reactionFaker;
    private readonly IServiceScopeFactory _scopeFactory;

    private static readonly long[] EmojiCodes =
    {
        128512,
        128514,
        128516,
        128519,
        128525,
        128512,
        128517,
        128540,
        128565,
        128562,
    };

    public ChatMessageReactionsSeeder(
        IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _reactionFaker = new Faker<ChatMessageReaction>()
            .RuleFor(r => r.Id, _ => Guid.NewGuid())
            .RuleFor(r => r.CreatedAt, f => f.Date.Past())
            .RuleFor(r => r.ReactionCode, f => f.Random.ArrayElement(EmojiCodes));
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        var messages = await mapper.FetchListAsync<ChatMessage>();
        var groups = await mapper.FetchListAsync<ChatGroup>();
        var members = await mapper.FetchListAsync<ChatGroupMember>();
        var users = await mapper.FetchListAsync<ChatifyUser>();

        var reactions = _reactionFaker.Generate(300);

        foreach ( var message in messages )
        {
            foreach ( var _ in Enumerable.Range(0, Random.Shared.Next(0, 10)) )
            {
                var reaction = _reactionFaker.Generate();

                var group = groups.FirstOrDefault(g => g.Id == message.ChatGroupId);
                var groupMembers = members
                    .Where(m => m.ChatGroupId == group.Id)
                    .ToList();
                var groupMessages = messages
                    .Where(m => m.ChatGroupId == group.Id)
                    .ToList();

                if ( group is null || !groupMembers.Any() || !groupMessages.Any() ) continue;

                reaction.ChatGroupId = group.Id;
                reaction.MessageId = message.Id;
                reaction.UserId = groupMembers[Random.Shared.Next(0, groupMembers.Count)].UserId;
                reaction.Username = users.FirstOrDefault(_ => _.Id == reaction.UserId)?.UserName;

                var reactionCounts = await mapper.FirstOrDefaultAsync<Dictionary<long, long>>(
                    "SELECT reaction_counts FROM chat_message_reactions WHERE message_id = ?;",
                    reaction.MessageId) ?? new Dictionary<long, long>();

                if ( !reactionCounts.ContainsKey(reaction.ReactionCode) ) reactionCounts[reaction.ReactionCode] = 0;
                reactionCounts[reaction.ReactionCode]++;
                reaction.ReactionCounts = reactionCounts;

                await mapper.InsertAsync(reaction, insertNulls: false);

                // Update Message Reaction Counts:
                var chatMessage = groupMessages.FirstOrDefault(m => m.Id == reaction.MessageId)!;
                if ( !chatMessage.ReactionCounts.ContainsKey(reaction.ReactionCode) )
                {
                    chatMessage.ReactionCounts[reaction.ReactionCode] = 0;
                }

                chatMessage.ReactionCounts[reaction.ReactionCode]++;

                await mapper.InsertAsync(chatMessage);
            }
        }
    }
}
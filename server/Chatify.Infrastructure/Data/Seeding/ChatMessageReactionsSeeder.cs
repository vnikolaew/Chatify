using Bogus;
using Cassandra.Mapping;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Chatify.Infrastructure.Data.Seeding;

internal sealed class ChatMessageReactionsSeeder(IServiceScopeFactory scopeFactory)
    : BaseSeeder<ChatMessageReaction>(scopeFactory)
{
    public override int Priority => 6;

    private readonly Faker<ChatMessageReaction> _reactionFaker = new Faker<ChatMessageReaction>()
        .RuleFor(r => r.Id, _ => Guid.NewGuid())
        .RuleFor(r => r.CreatedAt, f => f.Date.Past())
        .RuleFor(r => r.ReactionCode, f => f.Random.ArrayElement(EmojiCodes));

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

    protected override async Task SeedCoreAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = ScopeFactory.CreateAsyncScope();

        var dbMapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var repo = scope.ServiceProvider.GetRequiredService<IChatMessageReactionRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<AutoMapper.IMapper>();

        var messages = await dbMapper.FetchListAsync<ChatMessage>();
        var groups = await dbMapper.FetchListAsync<ChatGroup>();
        var members = await dbMapper.FetchListAsync<ChatGroupMember>();
        var users = await dbMapper.FetchListAsync<ChatifyUser>();

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

                var reactionCounts = await dbMapper.FirstOrDefaultAsync<Dictionary<long, long>>(
                    "SELECT reaction_counts FROM chat_message_reactions WHERE message_id = ?;",
                    reaction.MessageId) ?? new Dictionary<long, long>();

                if ( !reactionCounts.ContainsKey(reaction.ReactionCode) ) reactionCounts[reaction.ReactionCode] = 0;
                reactionCounts[reaction.ReactionCode]++;
                reaction.ReactionCounts = reactionCounts;

                await dbMapper.InsertAsync(reaction, insertNulls: true);

                // Update Message Reaction Counts:
                var chatMessage = groupMessages.FirstOrDefault(m => m.Id == reaction.MessageId)!;
                if ( !chatMessage.ReactionCounts.ContainsKey(reaction.ReactionCode) )
                {
                    chatMessage.ReactionCounts[reaction.ReactionCode] = 0;
                }

                chatMessage.ReactionCounts[reaction.ReactionCode]++;

                await dbMapper.InsertAsync(chatMessage);
            }
        }
    }
}
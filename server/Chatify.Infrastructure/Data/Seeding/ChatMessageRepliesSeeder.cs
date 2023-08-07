using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Chatify.Infrastructure.Data.Seeding;

internal sealed class ChatMessageRepliesSeeder : ChatGroupMessageSeeder
{
    public new int Priority => 5;

    public ChatMessageRepliesSeeder(IServiceScopeFactory scopeFactory)
        : base(scopeFactory)
    {
    }

    public new async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = ScopeFactory.CreateAsyncScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        var messages = (await mapper.FetchAsync<ChatMessage>()).ToList();
        var replies = MessageFaker
            .Generate(500)
            .Select(m => new ChatMessageReply
            {
                Id = m.Id,
                CreatedAt = m.CreatedAt.AddDays(Random.Shared.Next(1, 100)),
                ChatGroupId = m.ChatGroupId,
                UserId = m.UserId,
                Content = m.Content,
                Metadata = m.Metadata,
                ReactionCounts = m.ReactionCounts,
                UpdatedAt = m.UpdatedAt,
                ReplyToId = messages[Random.Shared.Next(0, messages.Count)].Id
            }).ToList();
        
        foreach ( var reply in replies )
        {
            await mapper.InsertAsync(reply, insertNulls: true);
            await mapper.ExecuteAsync(
                "UPDATE chat_messages_reply_count SET reply_count = reply_count + 1 WHERE id = ?",
                reply.ReplyToId);
        }
    }
}
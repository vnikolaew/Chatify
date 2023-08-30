using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Chatify.Infrastructure.Data.Seeding;

internal sealed class ChatMessageRepliesSeeder(IServiceScopeFactory scopeFactory)
    : ISeeder
{
    public int Priority => 5;

    public new async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        var messages = await mapper.FetchListAsync<ChatMessage>();
        var groupMembers = await mapper.FetchListAsync<ChatGroupMember>();

        var replyCounters = new Dictionary<Guid, long>();
        foreach ( var chatMessage in messages )
        {
            var members = groupMembers
                .Where(m => m.ChatGroupId == chatMessage.ChatGroupId)
                .ToList();

            var repliesCount = Random.Shared.Next(0, 6);
            foreach ( var _ in Enumerable.Range(0, repliesCount) )
            {
                var replyMessage = ChatGroupMessageSeeder.MessageFaker.Generate();

                replyCounters.TryAdd(chatMessage.Id, 0);
                replyCounters[chatMessage.Id]++;
                var reply = new ChatMessageReply
                {
                    Id = replyMessage.Id,
                    CreatedAt = chatMessage.CreatedAt.Add(TimeSpan.FromDays(Random.Shared.Next(0, 3))),
                    ChatGroupId = chatMessage.ChatGroupId,
                    UserId = members[Random.Shared.Next(0, members.Count)].UserId,
                    Content = replyMessage.Content,
                    Metadata = replyMessage.Metadata,
                    ReactionCounts = replyMessage.ReactionCounts,
                    UpdatedAt = replyMessage.UpdatedAt,
                    ReplyToId = chatMessage.Id,
                    RepliesCount = replyCounters[chatMessage.Id]
                };

                await mapper.InsertAsync(reply, insertNulls: true);
                await IncrementMessageRepliesCount(mapper, reply.ReplyToId);
                try
                {
                    await UpdateReplierSummaries(mapper, reply);
                }
                catch ( Exception )
                {
                }
            }
        }

        await UpdateChatMessageReplySummariesTotals(mapper);
    }

    private static async Task UpdateChatMessageReplySummariesTotals(
        IMapper mapper)
    {
        // Get reply counts for all messages:
        var replyCounts =
            ( await mapper.FetchListAsync<ChatMessageReplyCount>(
                "SELECT COUNT(*) as Count, reply_to_id as MessageId FROM chat_message_replies GROUP BY reply_to_id;") )
            .ToDictionary(_ => _.MessageId, _ => _.Count);

        var replySummaries = await mapper
            .FetchListAsync<ChatMessageRepliesSummary>();
        foreach ( var repliesSummary in replySummaries )
        {
            repliesSummary.Total = replyCounts.TryGetValue(
                repliesSummary.MessageId, out var count) ? count : 0;
            await mapper.UpdateAsync<ChatMessageRepliesSummary>("SET total = ? WHERE chat_group_id = ? AND created_at = ? IF message_id = ?",
                repliesSummary.Total,
                repliesSummary.ChatGroupId,
                repliesSummary.CreatedAt,
                repliesSummary.MessageId);
        }
    }

    private class ChatMessageReplyCount
    {
        public Guid MessageId { get; set; }
        public long Count { get; set; }
    }

    private static async Task UpdateReplierSummaries(IMapper mapper,
        ChatMessageReply reply)
    {
        var message = await mapper.FirstOrDefaultAsync<ChatMessage>(
            "SELECT id, created_at FROM chat_messages WHERE id = ? ALLOW FILTERING ;",
            reply.ReplyToId);

        // Update `reply_summaries` view table:
        var replierIds = await mapper.FirstOrDefaultAsync<System.Collections.Generic.HashSet<Guid>>(
            "SELECT replier_ids FROM chat_message_replies_summaries WHERE message_id = ? ALLOW FILTERING;",
            reply.ReplyToId);

        var userInfo = await mapper.FirstOrDefaultAsync<ChatifyUser>(
            "SELECT id, username, profile_picture FROM users WHERE id = ?;", reply.UserId);
        if ( replierIds is not null )
        {
            if ( !replierIds.Contains(reply.UserId) )
            {
                await mapper.UpdateAsync<ChatMessageRepliesSummary>(
                    $" SET replier_ids = replier_ids + ?," +
                    $" updated_at = ?, " +
                    $"updated = ?," +
                    $" replier_infos = replier_infos + ?" +
                    $" WHERE chat_group_id = ? AND created_at = ? IF message_id = ?;",
                    new HashSet<Guid> { userInfo.Id },
                    reply.CreatedAt,
                    true,
                    new HashSet<MessageReplierInfo>
                    {
                        new()
                        {
                            UserId = userInfo.Id,
                            Username = userInfo.UserName,
                            ProfilePictureUrl = userInfo.ProfilePicture.MediaUrl
                        }
                    },
                    reply.ChatGroupId,
                    message.CreatedAt,
                    message.Id
                );
            }
            else
            {
                await mapper.UpdateAsync<ChatMessageRepliesSummary>(
                    " SET updated_at = ?, updated = ? WHERE created_at = ? AND chat_group_id = ? IF message_id = ?;",
                    reply.CreatedAt,
                    true,
                    message.CreatedAt,
                    reply.ChatGroupId,
                    message.Id
                );
            }
        }
    }

    private static async Task IncrementMessageRepliesCount(IMapper mapper,
        Guid messageId)
    {
        // Update `chat_message_replies_count` table:
        await mapper.ExecuteAsync(
            "UPDATE chat_messages_reply_count SET reply_count = reply_count + 1 WHERE message_id = ?",
            messageId);
    }
}
using System.Text.Json;
using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using LanguageExt;
using LanguageExt.Common;
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

        var replies = ChatGroupMessageSeeder
            .MessageFaker
            .Generate(500)
            .Select(m =>
            {
                var message = messages[Random.Shared.Next(0, messages.Count)];
                var members = groupMembers
                    .Where(m => m.ChatGroupId == message.ChatGroupId)
                    .ToList();
                return new ChatMessageReply
                {
                    Id = m.Id,
                    CreatedAt = message.CreatedAt.Add(TimeSpan.FromDays(Random.Shared.Next(0, 3))),
                    ChatGroupId = message.ChatGroupId,
                    UserId = members[Random.Shared.Next(0, members.Count)].UserId,
                    Content = m.Content,
                    Metadata = m.Metadata,
                    ReactionCounts = m.ReactionCounts,
                    UpdatedAt = m.UpdatedAt,
                    ReplyToId = message.Id
                };
            }).ToList();

        foreach ( var reply in replies )
        {
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

    private static async Task<Unit> UpdateReplierSummaries(
        IMapper mapper, ChatMessageReply reply)
    {
        // Update `reply_summaries` view table:
        var replierIds = await mapper.FirstOrDefaultAsync<System.Collections.Generic.HashSet<Guid>>(
            "SELECT replier_ids FROM chat_message_replies_summaries WHERE message_id = ? ALLOW FILTERING;",
            reply.ReplyToId);

        var userInfo = await mapper.FirstOrDefaultAsync<ChatifyUser>(
            "SELECT id, username, profile_picture FROM users WHERE id = ?;", reply.UserId);

        if ( replierIds is not null )
        {
            var total = await mapper.FirstOrDefaultAsync<long?>(
                "SELECT total FROM chat_message_replies_summaries WHERE message_id = ?",
                reply.ReplyToId) ?? 0;

            if ( !replierIds.Contains(reply.UserId) )
            {
                await mapper.UpdateAsync<ChatMessageRepliesSummary>(
                    $" SET replier_ids = replier_ids + ?," +
                    $" updated_at = ?, " +
                    $"updated = ?," +
                    $" replier_infos = replier_infos + ?," +
                    $" total = ? WHERE chat_group_id = ? AND created_at = ? IF message_id = ?;",
                    new System.Collections.Generic.HashSet<Guid> { userInfo.Id },
                    reply.CreatedAt,
                    true,
                    new System.Collections.Generic.HashSet<MessageReplierInfo>
                    {
                        new()
                        {
                            UserId = userInfo.Id,
                            Username = userInfo.UserName,
                            ProfilePictureUrl = userInfo.ProfilePicture.MediaUrl
                        }
                    },
                    total + 1,
                    reply.ChatGroupId,
                    reply.CreatedAt,
                    reply.ReplyToId
                );
            }
            else
            {
                await mapper.UpdateAsync<ChatMessageRepliesSummary>(
                    """
                    SET total = ?, updated_at = ?,
                    updated = ? WHERE created_at = ? AND chat_group_id = ? IF message_id = ?;"
                    """,
                    total + 1,
                    reply.CreatedAt,
                    true,
                    reply.CreatedAt,
                    reply.ChatGroupId,
                    reply.ReplyToId
                );
            }
        }
        else
        {
            var messageRepliesSummary = new ChatMessageRepliesSummary
            {
                Id = Guid.NewGuid(),
                ChatGroupId = reply.ChatGroupId,
                CreatedAt = reply.CreatedAt,
                Total = 1,
                MessageId = reply.ReplyToId,
                ReplierIds = new System.Collections.Generic.HashSet<Guid> { reply.UserId },
                ReplierInfos = new System.Collections.Generic.HashSet<MessageReplierInfo>
                {
                    new()
                    {
                        UserId = userInfo.Id,
                        Username = userInfo.UserName,
                        ProfilePictureUrl = userInfo.ProfilePicture.MediaUrl
                    }
                }
            };
            await mapper.InsertAsync(messageRepliesSummary);
        }

        return Unit.Default;
    }

    private static async Task IncrementMessageRepliesCount(IMapper mapper, Guid messageId)
    {
        var replyCount = await mapper.FirstOrDefaultAsync<long>(
            "SELECT replies_count FROM chat_message_replies WHERE reply_to_id = ?;",
            messageId);

        await mapper.ExecuteAsync(
            "UPDATE chat_message_replies SET replies_count = ? WHERE reply_to_id = ?",
            replyCount + 1,
            messageId);

        // Update `chat_message_replies_count` table:
        await mapper.ExecuteAsync(
            "UPDATE chat_messages_reply_count SET reply_count = reply_count + 1 WHERE message_id = ?",
            messageId);
    }
}
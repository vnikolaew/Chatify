using Bogus;
using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Chatify.Infrastructure.Data.Seeding;

internal sealed class ChatGroupMessageSeeder(IServiceScopeFactory scopeFactory)
    : ISeeder
{
    public int Priority => 4;

    public static readonly Faker<ChatMessage> MessageFaker
        = new Faker<ChatMessage>()
            .RuleFor(m => m.Id, _ => Guid.NewGuid())
            .RuleFor(m => m.Attachments,
                f => AddAttachments
                    ? new Media[]
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            MediaUrl = f.Internet.Avatar(),
                        }
                    }
                    : Array.Empty<Media>())
            .RuleFor(m => m.CreatedAt, f => f.Date.Past())
            .RuleFor(m => m.Content, f => f.Lorem.Sentences(2))
            .RuleFor(m => m.ReactionCounts, _ => new Dictionary<short, long>());

    private static bool AddAttachments
        => Random.Shared.NextDouble() > 0.50;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var cache = scope.ServiceProvider.GetRequiredService<IDatabase>();

        var members = await mapper
            .FetchListAsync<ChatGroupMember>("SELECT * FROM chat_group_members;");

        var messages = MessageFaker.Generate(100);
        foreach ( var message in messages )
        {
            var member = members[Random.Shared.Next(0, members.Count)];
            message.UserId = member.UserId;
            message.ChatGroupId = member.ChatGroupId;

            await mapper.InsertAsync(message, insertNulls: true);
            if ( message.Attachments.Any() ) await InsertAttachments(message, mapper);
        }

        await UpdateUserFeedCaches(mapper, cache);
    }

    private static async Task InsertAttachments(ChatMessage message, IMapper mapper)
    {
        var user = await mapper.FirstOrDefaultAsync<ChatifyUser>("WHERE id = ?", message.UserId);
        var groupAttachments = message
            .Attachments
            .Select(a => new ChatGroupAttachment
            {
                CreatedAt = message.CreatedAt.DateTime,
                ChatGroupId = message.ChatGroupId,
                UserId = message.UserId,
                Username = user.UserName,
                AttachmentId = a.Id,
                MediaInfo = a
            });

        foreach ( var attachment in groupAttachments )
        {
            await mapper.InsertAsync(attachment);
        }
    }

    private static async Task UpdateUserFeedCaches(
        IMapper mapper,
        IDatabase cache)
    {
        var latestMessages = await mapper.FetchListAsync<ChatMessage>(
            "SELECT * FROM chat_messages PER PARTITION LIMIT 1;");

        // Fill user cache feed sorted sets with latest message from each chat group:
        foreach ( var latestMessage in latestMessages )
        {
            // Fetch all group members for each message:
            var groupMembers = await mapper.FetchListAsync<ChatGroupMember>(
                    " WHERE chat_group_id = ?", latestMessage.ChatGroupId);

            // Update User Feed for each group members (Sorted Set):
            foreach ( var groupMember in groupMembers )
            {
                var userFeedCacheKey = new RedisKey($"user:{groupMember.UserId}:feed");
                await cache.SortedSetAddAsync(
                    userFeedCacheKey,
                    new RedisValue(
                        latestMessage.ChatGroupId.ToString()
                    ), latestMessage.CreatedAt.Ticks);
            }
        }
    }
}
using AutoMapper;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Mappings;
using Chatify.Infrastructure.Data.Services;
using StackExchange.Redis;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class ChatMessageReactionRepository(IMapper mapper, Mapper dbMapper, IDatabase cache,
        IEntityChangeTracker changeTracker, string? idColumn = default)
    : BaseCassandraRepository<ChatMessageReaction, Models.ChatMessageReaction, Guid>(mapper, dbMapper, changeTracker,
            idColumn),
        IChatMessageReactionRepository
{
    public new async Task<ChatMessageReaction> SaveAsync(
        ChatMessageReaction messageReaction,
        CancellationToken cancellationToken = default)
    {
        var model = await base.SaveAsync(messageReaction, cancellationToken);

        var messageReactionsKey = new RedisKey($"message-reactions:{messageReaction.MessageId}");
        var userId = new RedisValue(messageReaction.UserId.ToString());

        await cache.SetAddAsync(messageReactionsKey, userId);
        return model;
    }

    public new async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var messageReaction = await GetAsync(id, cancellationToken);
        if ( messageReaction is null ) return false;

        var messageReactionsKey = new RedisKey($"message-reactions:{messageReaction.MessageId}");
        var userId = new RedisValue(messageReaction.UserId.ToString());

        var removeTasks = new[]
        {
            base.DeleteAsync(id, cancellationToken),
            cache.SetRemoveAsync(messageReactionsKey, userId)
        };

        var results = await Task.WhenAll(removeTasks);
        return results.All(_ => _);
    }

    public async Task<bool> Exists(
        Guid messageId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var messageReactionsKey = new RedisKey($"message-reactions:{messageId}");
        var userIdValue = new RedisValue(userId.ToString());

        return await cache.SetContainsAsync(messageReactionsKey, userIdValue);
    }

    public Task<ChatMessageReaction?> ByMessageAndUser(Guid messageId, Guid userId,
        CancellationToken cancellationToken = default)
        => DbMapper.FirstOrDefaultAsync<Models.ChatMessageReaction>(
                " WHERE message_id = ? AND user_id = ? ALLOW FILTERING", messageId, userId)
            .ToAsyncNullable<Models.ChatMessageReaction, ChatMessageReaction>(Mapper);
}
﻿using AutoMapper;
using Chatify.Domain.Entities;
using StackExchange.Redis;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class ChatMessageReactionRepository
    : BaseCassandraRepository<ChatMessageReaction, Models.ChatMessageReaction, Guid>,
        IChatMessageReactionRepository
{
    private readonly IDatabase _cache;

    public ChatMessageReactionRepository(
        IMapper mapper, Mapper dbMapper, IDatabase cache, string? idColumn = default)
        : base(mapper, dbMapper, idColumn)
        => _cache = cache;

    public new async Task<ChatMessageReaction> SaveAsync(
        ChatMessageReaction messageReaction,
        CancellationToken cancellationToken = default)
    {
        var model = await base.SaveAsync(messageReaction, cancellationToken);

        var messageReactionsKey = new RedisKey($"message-reactions:{messageReaction.MessageId}");
        var userId = new RedisValue(messageReaction.UserId.ToString());

        await _cache.SetAddAsync(messageReactionsKey, userId);
        return model;
    }

    public new async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var messageReaction = await GetAsync(id, cancellationToken);
        if (messageReaction is null) return false;

        var success = await base.DeleteAsync(id, cancellationToken);
        if (!success) return false;

        var messageReactionsKey = new RedisKey($"message-reactions:{messageReaction.MessageId}");
        var userId = new RedisValue(messageReaction.UserId.ToString());

        await _cache.SetRemoveAsync(messageReactionsKey, userId);
        return true;
    }

    public async Task<bool> Exists(
        Guid messageId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var messageReactionsKey = new RedisKey($"message-reactions:{messageId}");
        var userIdValue = new RedisValue(userId.ToString());
        
        return await _cache.SetContainsAsync(messageReactionsKey, userIdValue);
    }

    public async Task<ChatMessageReaction?> ByMessageAndUser(Guid messageId, Guid userId, CancellationToken cancellationToken = default)
    {
        var reaction =
            await DbMapper.FirstOrDefaultAsync<Models.ChatMessageReaction>(
                " WHERE message_id = ? AND user_id = ? ALLOW FILTERING", messageId, userId);
        
        return Mapper.Map<ChatMessageReaction>(reaction);
    }
}
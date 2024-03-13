using AutoMapper;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Caching.Extensions;
using Chatify.Infrastructure.Common.Mappings;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Services;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Humanizer;
using StackExchange.Redis;
using Guid = System.Guid;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class ChatMessageReactionRepository(
    IMapper mapper,
    Mapper dbMapper,
    IDatabase cache,
    IEntityChangeTracker changeTracker)
    : BaseCassandraRepository<ChatMessageReaction, Models.ChatMessageReaction, Guid>(mapper, dbMapper, changeTracker,
            nameof(Models.ChatMessageReaction.Id).Underscore()),
        IChatMessageReactionRepository
{
    public new async Task<ChatMessageReaction> SaveAsync(
        ChatMessageReaction messageReaction,
        CancellationToken cancellationToken = default)
    {
        var model = await base.SaveAsync(messageReaction, cancellationToken);

        // Add userId to Reactions set:
        var messageReactionsKey = messageReaction.MessageId.GetMessageReactionsKey();
        var userId = new RedisValue(messageReaction.UserId.ToString());

        // Add messageId -> reactionCode to User reactions Hash:
        await new[]
        {
            // Add user to message reactors:
            cache.SetAddAsync(messageReactionsKey, userId),

            // Add message and reaction type to users reactions:
            cache.AddUserReactionAsync(
                messageReaction.UserId,
                messageReaction.MessageId,
                messageReaction.ReactionCode)
        };

        return model;
    }

    public new async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var messageReaction = await GetAsync(id, cancellationToken);
        if ( messageReaction is null ) return false;

        var messageReactionsKey = messageReaction.MessageId.GetMessageReactionsKey();
        var userId = new RedisValue(messageReaction.UserId.ToString());

        // Delete messageId -> reactionCode to User reactions Hash:
        var removeTasks = new[]
        {
            base.DeleteAsync(messageReaction, cancellationToken),
            cache.SetRemoveAsync(messageReactionsKey, userId),
            cache.RemoveUserReactionAsync(
                messageReaction.UserId,
                messageReaction.MessageId)
        };

        var results = await removeTasks;
        return results.All(_ => _);
    }

    public async Task<bool> Exists(
        Guid messageId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var messageReactionsKey = messageId.GetMessageReactionsKey();
        var userIdValue = new RedisValue(userId.ToString());

        var userReactionsKey = userId.GetUserReactionsKey();
        var existTasks = await new[]
        {
            cache.SetContainsAsync(messageReactionsKey, userIdValue),
            cache.HashExistsAsync(userReactionsKey, messageId.ToString())
        };

        return existTasks.Any(_ => _);
    }

    public async Task<List<ChatMessageReaction>?> AllForMessage(
        Guid messageId,
        CancellationToken cancellationToken = default)
        => ( await DbMapper.FetchListAsync<Models.ChatMessageReaction>("WHERE message_id = ?", messageId) )
            .ToList<ChatMessageReaction>(Mapper);

    public async Task<IDictionary<Guid, long?>> AllByUserAndMessageIds(
        Guid userId,
        IEnumerable<Guid> messageIds,
        CancellationToken cancellationToken = default)
    {
        var messageIdValues = messageIds
            .Select(id => new RedisValue(id.ToString()))
            .ToArray();

        var reactionCodes = await cache.GetUserReactionsAsync(
            userId,
            messageIdValues
        );

        return messageIds
            .Zip(reactionCodes)
            .ToDictionary(pair => pair.Item1, pair => pair.Item2 ?? null);
    }

    public Task<ChatMessageReaction?> ByMessageAndUser(Guid messageId,
        Guid userId,
        CancellationToken cancellationToken = default)
        => DbMapper
            .FirstOrDefaultAsync<Models.ChatMessageReaction>(
                " WHERE message_id = ? AND user_id = ? ALLOW FILTERING", messageId, userId)!
            .ToAsyncNullable<Models.ChatMessageReaction, ChatMessageReaction>(Mapper);
}
﻿using Chatify.Domain.Events.Groups;
using Chatify.Infrastructure.Common.Caching.Extensions;
using Chatify.Infrastructure.Data.Models;
using Chatify.Shared.Abstractions.Events;
using Redis.OM.Contracts;
using Redis.OM.Searching;
using StackExchange.Redis;
using IMapper = Cassandra.Mapping.IMapper;

namespace Chatify.Infrastructure.ChatGroups.EventHandlers;

internal sealed class ChatGroupCreatedEventCacheFillHandler
(IRedisConnectionProvider connectionProvider,
    IMapper mapper,
    IDatabase cache) : IEventHandler<ChatGroupCreatedEvent>
{
    private readonly IRedisCollection<ChatGroup> _cacheGroups
        = connectionProvider.RedisCollection<ChatGroup>();

    public async Task HandleAsync(ChatGroupCreatedEvent @event,
        CancellationToken cancellationToken = default)
    {
        var chatGroup = await mapper.FirstOrDefaultAsync<ChatGroup>(
            " WHERE id = ?", @event.GroupId);
        if ( chatGroup is null ) return;

        await _cacheGroups.InsertAsync(chatGroup);

        // Update User Feed (Sorted Set):
        await cache.AddUserFeedEntryAsync(
            @event.CreatorId,
            @event.GroupId,
            @event.Timestamp);
    }
}
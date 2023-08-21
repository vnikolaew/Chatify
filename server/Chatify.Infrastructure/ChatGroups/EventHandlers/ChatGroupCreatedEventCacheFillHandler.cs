using Chatify.Domain.Events.Groups;
using Chatify.Infrastructure.Data.Models;
using Chatify.Shared.Abstractions.Events;
using Redis.OM.Contracts;
using Redis.OM.Searching;
using IMapper = Cassandra.Mapping.IMapper;

namespace Chatify.Infrastructure.ChatGroups.EventHandlers;

internal sealed class ChatGroupCreatedEventCacheFillHandler
    (IRedisConnectionProvider connectionProvider, IMapper mapper) : IEventHandler<ChatGroupCreatedEvent>
{
    private readonly IRedisCollection<ChatGroup> _cacheGroups
        = connectionProvider.RedisCollection<ChatGroup>();

    public async Task HandleAsync(ChatGroupCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        var chatGroup = await mapper.FirstOrDefaultAsync<ChatGroup>(
            " WHERE id = ?", @event.GroupId);
        if(chatGroup is null) return;

        await _cacheGroups.InsertAsync(chatGroup);
    }
}
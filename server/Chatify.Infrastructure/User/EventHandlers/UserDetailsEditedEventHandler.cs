using Chatify.Domain.Events.Users;
using Chatify.Infrastructure.Data.Models;
using Chatify.Shared.Abstractions.Events;
using Redis.OM.Contracts;
using Redis.OM.Searching;
using IMapper = Cassandra.Mapping.IMapper;

namespace Chatify.Infrastructure.User.EventHandlers;

internal sealed class UserDetailsEditedEventHandler(
        IRedisConnectionProvider connectionProvider,
        IMapper dbMapper
    )
    : IEventHandler<UserDetailsEditedEvent>
{
    private readonly IRedisCollection<ChatifyUser> _cacheUsers = connectionProvider.RedisCollection<ChatifyUser>();

    public async Task HandleAsync(
        UserDetailsEditedEvent @event,
        CancellationToken cancellationToken = default)
    {
        var user = await dbMapper.FirstOrDefaultAsync<ChatifyUser>("WHERE id = ?;", @event.UserId);
        if ( user is null ) return;

        // Invalidate stale User cache info:
        await _cacheUsers.UpdateAsync(user);
    }
}
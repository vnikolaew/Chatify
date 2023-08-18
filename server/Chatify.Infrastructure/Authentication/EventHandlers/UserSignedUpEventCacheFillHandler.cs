using Chatify.Domain.Events.Users;
using Chatify.Infrastructure.Data.Models;
using Chatify.Shared.Abstractions.Events;
using Redis.OM.Contracts;
using Redis.OM.Searching;
using IMapper = Cassandra.Mapping.IMapper;

namespace Chatify.Infrastructure.Authentication.EventHandlers;

internal sealed class UserSignedUpEventCacheFillHandler(
    IMapper dbMapper,
    IRedisConnectionProvider connectionProvider)
    : IEventHandler<UserSignedUpEvent>
{
    private readonly IRedisCollection<ChatifyUser> _cacheUsers
        = connectionProvider.RedisCollection<ChatifyUser>();
    
    public async Task HandleAsync(
        UserSignedUpEvent @event,
        CancellationToken cancellationToken = default)
    {
        var user = await dbMapper.FirstOrDefaultAsync<ChatifyUser>(
            " WHERE id = ?", @event.UserId.ToString());
        if(user is null) return;

        await _cacheUsers.InsertAsync(user);
    }
}
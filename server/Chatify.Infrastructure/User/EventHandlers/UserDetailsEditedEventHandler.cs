using AutoMapper;
using Chatify.Domain.Events.Users;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Caching.Extensions;
using Chatify.Infrastructure.Common.Mappings;
using Chatify.Shared.Abstractions.Events;
using StackExchange.Redis;

namespace Chatify.Infrastructure.User.EventHandlers;

internal sealed class UserDetailsEditedEventHandler(IDatabase cache,
        IUserRepository users,
        IMapper mapper)
    : IEventHandler<UserDetailsEditedEvent>
{
    public async Task HandleAsync(
        UserDetailsEditedEvent @event,
        CancellationToken cancellationToken = default)
    {
        var user = await users.GetAsync(@event.UserId, cancellationToken);
        if ( user is null ) return;

        // Invalidate stale User cache info:
        await cache.SetAsync($"user:{user.Id}", user.To<Domain.Entities.User>(mapper));
    }
}
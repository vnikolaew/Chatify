using AutoMapper;
using Chatify.Domain.Events.Users;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Caching.Extensions;
using Chatify.Infrastructure.Common.Mappings;
using Chatify.Shared.Abstractions.Events;
using StackExchange.Redis;

namespace Chatify.Infrastructure.User.EventHandlers;

internal sealed class UserDetailsEditedEventHandler
    : IEventHandler<UserDetailsEditedEvent>
{
    private readonly IDatabase _cache;
    private readonly IMapper _mapper;
    private readonly IUserRepository _users;

    public UserDetailsEditedEventHandler(
        IDatabase cache,
        IUserRepository users,
        IMapper mapper)
    {
        _cache = cache;
        _users = users;
        _mapper = mapper;
    }

    public async Task HandleAsync(
        UserDetailsEditedEvent @event,
        CancellationToken cancellationToken = default)
    {
        var user = await _users.GetAsync(@event.UserId, cancellationToken);
        if ( user is null ) return;

        // Invalidate stale User cache info:
        await _cache.SetAsync($"user:{user.Id}", user.To<Domain.Entities.User>(_mapper));
    }
}
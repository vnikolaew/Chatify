using Chatify.Domain.Events.Friendships;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Events;

namespace Chatify.Infrastructure.Friendships.EventHandlers;

internal sealed class UserUnfriendedEventHandler : IEventHandler<UserUnfriendedEvent>
{
    private readonly IChatGroupRepository _groups;

    public UserUnfriendedEventHandler(IChatGroupRepository groups)
    {
        _groups = groups;
    }

    public async Task HandleAsync(
        UserUnfriendedEvent @event, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
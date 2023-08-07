using Chatify.Domain.Common;
using Chatify.Domain.Events.Users;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using Microsoft.Extensions.Logging;

namespace Chatify.Application.Authentication.EventHandlers;

internal sealed class UserSignedInHandler : IEventHandler<UserSignedInEvent>
{
    private readonly ILogger<UserSignedInHandler> _logger;
    private readonly IDomainRepository<Domain.Entities.User, Guid> _users;
    private readonly IClock _clock;

    public UserSignedInHandler(
        ILogger<UserSignedInHandler> logger,
        IClock clock,
        IDomainRepository<Domain.Entities.User, Guid> userRepo)
    {
        _logger = logger;
        _clock = clock;
        _users = userRepo;
    }

    public async Task HandleAsync(
        UserSignedInEvent @event,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User with Id '{Id}' signed in at {DateTime:u}",
            @event.UserId, _clock.Now);

        await _users.UpdateAsync(@event.UserId, user =>
        {
            user.LastLogin = _clock.Now;
            user.UpdatedAt = _clock.Now;
        }, cancellationToken);
    }
}
using Chatify.Domain.Events.Users;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using Microsoft.Extensions.Logging;

namespace Chatify.Application.Authentication.EventHandlers;

public sealed class UserSignedInHandler(
        ILogger<UserSignedInHandler> logger,
        IClock clock,
        IUserRepository userRepo)
    : IEventHandler<UserSignedInEvent>
{
    public async Task HandleAsync(
        UserSignedInEvent @event,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("User with Id '{Id}' signed in at {DateTime:u}",
            @event.UserId, clock.Now);

        await userRepo.UpdateAsync(@event.UserId, user =>
        {
            user.LastLogin = clock.Now;
            user.UpdatedAt = clock.Now;
        }, cancellationToken);
    }
}
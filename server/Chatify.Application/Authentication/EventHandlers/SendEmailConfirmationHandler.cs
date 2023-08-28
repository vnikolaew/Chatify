using Chatify.Application.Authentication.Contracts;
using Chatify.Domain.Events.Users;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Events;
using Microsoft.Extensions.Logging;

namespace Chatify.Application.Authentication.EventHandlers;

public sealed class SendEmailConfirmationHandler(
        ILogger<SendEmailConfirmationHandler> logger,
        IEmailConfirmationService emailConfirmationService,
        IUserRepository users)
    : IEventHandler<UserSignedUpEvent>
{
    public async Task HandleAsync(
        UserSignedUpEvent @event,
        CancellationToken cancellationToken = default)
    {
        var user = await users.GetAsync(@event.UserId, cancellationToken);
        if(user is null) return;
        
        logger.LogInformation("User with Id '{UserId}' successfully signed up", @event.UserId);
        var success = await emailConfirmationService
            .SendConfirmationEmailForUserAsync(user, cancellationToken);

        if (success)
        {
            logger.LogInformation(
                "Confirmation email successfully sent to user with Id '{Id}'",
                @event.UserId);
        }
    }
}
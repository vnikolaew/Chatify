using Chatify.Application.Authentication.Contracts;
using Chatify.Domain.Events;
using Chatify.Domain.Events.Users;
using Chatify.Shared.Abstractions.Events;
using Microsoft.Extensions.Logging;

namespace Chatify.Application.Authentication.EventHandlers;

internal sealed class SendEmailConfirmationHandler : IEventHandler<UserSignedUpEvent>
{
    private readonly ILogger<SendEmailConfirmationHandler> _logger;
    private readonly IEmailConfirmationService _emailConfirmationService;

    public SendEmailConfirmationHandler(
        ILogger<SendEmailConfirmationHandler> logger,
        IEmailConfirmationService emailConfirmationService)
    {
        _logger = logger;
        _emailConfirmationService = emailConfirmationService;
    }

    public async Task HandleAsync(
        UserSignedUpEvent @event,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User with Id '{UserId}' successfully signed up", @event.UserId);
        var success = await _emailConfirmationService
            .SendConfirmationEmailForUserAsync(@event.UserId, cancellationToken);

        if (success)
        {
            _logger.LogInformation(
                "Confirmation email successfully sent to user with Id '{Id}'",
                @event.UserId);
        }
    }
}
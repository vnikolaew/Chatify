using Chatify.Application.Authentication.Contracts;
using Chatify.Domain.Events.Users;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Events;
using Microsoft.Extensions.Logging;

namespace Chatify.Application.Authentication.EventHandlers;

public sealed class SendEmailConfirmationHandler
    : IEventHandler<UserSignedUpEvent>
{
    private readonly ILogger<SendEmailConfirmationHandler> _logger;
    private readonly IUserRepository _users;
    private readonly IEmailConfirmationService _emailConfirmationService;

    public SendEmailConfirmationHandler(
        ILogger<SendEmailConfirmationHandler> logger,
        IEmailConfirmationService emailConfirmationService,
        IUserRepository users)
    {
        _logger = logger;
        _emailConfirmationService = emailConfirmationService;
        _users = users;
    }

    public async Task HandleAsync(
        UserSignedUpEvent @event,
        CancellationToken cancellationToken = default)
    {
        var user = await _users.GetAsync(@event.UserId, cancellationToken);
        if(user is null) return;
        
        _logger.LogInformation("User with Id '{UserId}' successfully signed up", @event.UserId);
        var success = await _emailConfirmationService
            .SendConfirmationEmailForUserAsync(user, cancellationToken);

        if (success)
        {
            _logger.LogInformation(
                "Confirmation email successfully sent to user with Id '{Id}'",
                @event.UserId);
        }
    }
}
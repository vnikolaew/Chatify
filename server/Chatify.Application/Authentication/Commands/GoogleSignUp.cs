using System.ComponentModel.DataAnnotations;
using Chatify.Application.Authentication.Contracts;
using Chatify.Application.Authentication.Models;
using Chatify.Domain.Events.Users;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Events;
using LanguageExt;
using GoogleSignUpResult = OneOf.OneOf<Chatify.Application.Authentication.Models.SignUpError, LanguageExt.Unit>;

namespace Chatify.Application.Authentication.Commands;

// Note that Access Token is expected to come from Front-End Sign-Up flow
public sealed record GoogleSignUp([Required] string AccessToken) : ICommand<GoogleSignUpResult>;

internal sealed class GoogleSignUpHandler
    : ICommandHandler<GoogleSignUp, GoogleSignUpResult>
{
    private readonly IAuthenticationService _authService;
    private readonly IEventDispatcher _eventDispatcher;

    public GoogleSignUpHandler(
        IAuthenticationService authService,
        IEventDispatcher eventDispatcher)
    {
        _authService = authService;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<GoogleSignUpResult> HandleAsync(GoogleSignUp command,
        CancellationToken cancellationToken = default)
    {
        var result = await _authService
            .GoogleSignUpAsync(command, cancellationToken);

        if ( result.IsLeft ) return new SignUpError(result.LeftToArray()[0].Message);

        result.Do(async res =>
        {
            await _eventDispatcher.PublishAsync(new UserSignedUpEvent
            {
                Timestamp = DateTime.Now,
                UserId = res.UserId,
                AuthenticationProvider = res.AuthenticationProvider
            }, cancellationToken);
        });

        return Unit.Default;
    }
}
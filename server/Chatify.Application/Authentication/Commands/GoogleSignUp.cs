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

internal sealed class GoogleSignUpHandler(
        IAuthenticationService authService,
        IEventDispatcher eventDispatcher)
    : ICommandHandler<GoogleSignUp, GoogleSignUpResult>
{
    public async Task<GoogleSignUpResult> HandleAsync(GoogleSignUp command,
        CancellationToken cancellationToken = default)
    {
        var result = await authService
            .GoogleSignUpAsync(command, cancellationToken);
        if ( result.IsT0 ) return new SignUpError(result.AsT0.Message);

        var res = result.AsT1!;
        await eventDispatcher.PublishAsync(new UserSignedUpEvent
        {
            Timestamp = DateTime.Now,
            UserId = res.UserId,
            AuthenticationProvider = res.AuthenticationProvider
        }, cancellationToken);

        return Unit.Default;
    }
}
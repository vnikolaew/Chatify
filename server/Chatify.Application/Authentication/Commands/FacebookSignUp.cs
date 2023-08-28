using System.ComponentModel.DataAnnotations;
using Chatify.Application.Authentication.Contracts;
using Chatify.Application.Authentication.Models;
using Chatify.Domain.Events.Users;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Events;
using LanguageExt;
using FacebookSignUpResult = OneOf.OneOf<Chatify.Application.Authentication.Models.SignUpError, LanguageExt.Unit>;

namespace Chatify.Application.Authentication.Commands;

public sealed record FacebookSignUp([Required] string AccessToken)
    : ICommand<FacebookSignUpResult>;

internal sealed class FacebookSignUpHandler(
        IAuthenticationService authService,
        IEventDispatcher eventDispatcher)
    : ICommandHandler<FacebookSignUp, FacebookSignUpResult>
{
    public async Task<FacebookSignUpResult> HandleAsync(FacebookSignUp command,
        CancellationToken cancellationToken = default)
    {
        var result = await authService.FacebookSignUpAsync(command, cancellationToken);
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
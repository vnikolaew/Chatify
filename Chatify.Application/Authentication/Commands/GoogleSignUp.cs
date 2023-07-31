using System.ComponentModel.DataAnnotations;
using Chatify.Application.Authentication.Contracts;
using Chatify.Domain.Common;
using Chatify.Domain.Events;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Events;
using LanguageExt;
using LanguageExt.Common;
using GoogleSignUpResult = LanguageExt.Validation<LanguageExt.Common.Error, LanguageExt.Unit>;

namespace Chatify.Application.Authentication.Commands;

// Note that Access Token is expected to come from Front-End Sign-Up flow
public sealed record GoogleSignUp([Required] string AccessToken) : ICommand<GoogleSignUpResult>;

internal sealed class GoogleSignUpHandler : ICommandHandler<GoogleSignUp, GoogleSignUpResult>
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
        => await _authService
            .GoogleSignUpAsync(command, cancellationToken)
            .MapAsync(async result =>
            {
                await _eventDispatcher.PublishAsync(new UserSignedUpEvent
                {
                    Timestamp = DateTime.Now,
                    UserId = result.UserId,
                    AuthenticationProvider = result.AuthenticationProvider
                }, cancellationToken);
                return result;
            })
            .Select<Either<Error, UserSignedUpResult>, Validation<Error, Unit>>(
                res => res.Match(
                    _ => GoogleSignUpResult.Success(Unit.Default),
                    err => new Seq<Error>(new[] { err })));
}
using System.ComponentModel.DataAnnotations;
using Chatify.Application.Authentication.Contracts;
using Chatify.Domain.Events.Users;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Events;
using LanguageExt;
using LanguageExt.Common;
using FacebookSignUpResult = LanguageExt.Validation<LanguageExt.Common.Error, LanguageExt.Unit>;

namespace Chatify.Application.Authentication.Commands;

public sealed record FacebookSignUp([Required] string AccessToken)
    : ICommand<FacebookSignUpResult>;

internal sealed class FacebookSignUpHandler : ICommandHandler<FacebookSignUp, FacebookSignUpResult>
{
    private readonly IAuthenticationService _authService;
    private readonly IEventDispatcher _eventDispatcher;

    public FacebookSignUpHandler(
        IAuthenticationService authService,
        IEventDispatcher eventDispatcher)
    {
        _authService = authService;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<FacebookSignUpResult> HandleAsync(FacebookSignUp command,
        CancellationToken cancellationToken = default)
        => await _authService
            .FacebookSignUpAsync(command, cancellationToken)
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
                    _ => FacebookSignUpResult.Success(Unit.Default),
                    err => new Seq<Error>(new[] { err })));
}
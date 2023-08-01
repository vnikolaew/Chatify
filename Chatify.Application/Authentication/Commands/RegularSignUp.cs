using System.ComponentModel.DataAnnotations;
using Chatify.Application.Authentication.Contracts;
using Chatify.Domain.Events.Users;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Events;
using LanguageExt;
using LanguageExt.Common;
using RegularSignUpResult = LanguageExt.Validation<LanguageExt.Common.Error, LanguageExt.Unit>;

namespace Chatify.Application.Authentication.Commands;

public sealed record RegularSignUp(
    [Required] string Username,
    [EmailAddress, Required] string Email,
    [Required] [Password] string Password) : ICommand<RegularSignUpResult>;

internal sealed class RegularSignUpHandler : ICommandHandler<RegularSignUp, RegularSignUpResult>
{
    private readonly IAuthenticationService _authService;
    private readonly IEventDispatcher _eventDispatcher;

    public RegularSignUpHandler(
        IAuthenticationService authService,
        IEventDispatcher eventDispatcher)
    {
        _authService = authService;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<RegularSignUpResult> HandleAsync(
        RegularSignUp command,
        CancellationToken cancellationToken = default)
        => await _authService
            .RegularSignUpAsync(command, cancellationToken)
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
                    _ => RegularSignUpResult.Success(Unit.Default),
                    err => new Seq<Error>(new[] { err })));
}
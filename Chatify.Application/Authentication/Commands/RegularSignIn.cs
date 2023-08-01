using System.ComponentModel.DataAnnotations;
using Chatify.Application.Authentication.Contracts;
using Chatify.Domain.Events.Users;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Events;
using LanguageExt;
using LanguageExt.Common;
using RegularSignInResult = LanguageExt.Validation<LanguageExt.Common.Error, LanguageExt.Unit>;

namespace Chatify.Application.Authentication.Commands;

public sealed record RegularSignIn(
    [EmailAddress, Required] string Email,
    [Required] [Password] string Password) : ICommand<RegularSignInResult>;

internal sealed class RegularSignInHandler : ICommandHandler<RegularSignIn, RegularSignInResult>
{
    private readonly IAuthenticationService _authService;
    private readonly IEventDispatcher _eventDispatcher;

    public RegularSignInHandler(
        IAuthenticationService authService,
        IEventDispatcher eventDispatcher)
    {
        _authService = authService;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<RegularSignInResult> HandleAsync(
        RegularSignIn command,
        CancellationToken cancellationToken = default)
        => await _authService
            .RegularSignInAsync(command, cancellationToken)
            .MapAsync(async result =>
            {
                await _eventDispatcher.PublishAsync(new UserSignedInEvent
                {
                    Timestamp = DateTime.Now,
                    UserId = result.UserId,
                    AuthenticationProvider = result.AuthenticationProvider
                }, cancellationToken);
                return result;
            })
            .Select<Either<Error, UserSignedInResult>, Validation<Error, Unit>>(
                res => res.Match(
                    _ => RegularSignInResult.Success(Unit.Default),
                    err => new Seq<Error>(new[] { err })));
}
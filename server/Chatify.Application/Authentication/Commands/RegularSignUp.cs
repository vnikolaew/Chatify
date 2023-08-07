using System.ComponentModel.DataAnnotations;
using Chatify.Application.Authentication.Contracts;
using Chatify.Application.Authentication.Models;
using Chatify.Domain.Events.Users;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Events;
using LanguageExt;
using RegularSignUpResult = OneOf.OneOf<Chatify.Application.Authentication.Models.SignUpError, LanguageExt.Unit>;

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
    {
        var result =
            await _authService
                .RegularSignUpAsync(command, cancellationToken);

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
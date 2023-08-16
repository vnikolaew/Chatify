using System.ComponentModel.DataAnnotations;
using Chatify.Application.Authentication.Contracts;
using Chatify.Application.Authentication.Models;
using Chatify.Domain.Events.Users;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Events;
using LanguageExt;
using RegularSignInResult = OneOf.OneOf<Chatify.Application.Authentication.Models.SignInError, LanguageExt.Unit>;

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
    {
        var result = await _authService
            .RegularSignInAsync(command, cancellationToken);
        if ( result.IsT0 ) return new SignInError(result.AsT0.Message);

        var res = result.AsT1!;
        await _eventDispatcher.PublishAsync(new UserSignedInEvent
        {
            Timestamp = DateTime.Now,
            UserId = res.UserId,
            AuthenticationProvider = res.AuthenticationProvider
        }, cancellationToken);
        
        return Unit.Default;
    }
}
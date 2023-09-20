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
    [Required] [Password] string Password,
    [Required] bool RememberMe
    ) : ICommand<RegularSignInResult>;

public sealed class RegularSignInHandler(
        IAuthenticationService authService,
        IEventDispatcher eventDispatcher)
    : ICommandHandler<RegularSignIn, RegularSignInResult>
{
    public async Task<RegularSignInResult> HandleAsync(
        RegularSignIn command,
        CancellationToken cancellationToken = default)
    {
        var result = await authService
            .RegularSignInAsync(command, cancellationToken);
        if ( result.IsT0 ) return new SignInError(result.AsT0.Message);

        var res = result.AsT1!;
        await eventDispatcher.PublishAsync(new UserSignedInEvent
        {
            Timestamp = DateTime.Now,
            UserId = res.UserId,
            AuthenticationProvider = res.AuthenticationProvider
        }, cancellationToken);
        
        return Unit.Default;
    }
}
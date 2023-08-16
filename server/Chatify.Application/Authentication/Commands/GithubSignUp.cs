using System.ComponentModel.DataAnnotations;
using Chatify.Application.Authentication.Contracts;
using Chatify.Application.Authentication.Models;
using Chatify.Domain.Events.Users;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Events;
using LanguageExt;

namespace Chatify.Application.Authentication.Commands;

using GithubSignUpResult = OneOf.OneOf<SignUpError, Unit>;

public sealed record GithubSignUp([Required] string Code) : ICommand<GithubSignUpResult>;

internal sealed class GithubSignUpHandler : ICommandHandler<GithubSignUp, GithubSignUpResult>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IEventDispatcher _eventDispatcher;

    public GithubSignUpHandler(
        IAuthenticationService authenticationService,
        IEventDispatcher eventDispatcher)
    {
        _authenticationService = authenticationService;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<GithubSignUpResult> HandleAsync(
        GithubSignUp command, CancellationToken cancellationToken = default)
    {
        var result = await _authenticationService
            .GithubSignUpAsync(command, cancellationToken);

        if ( result.IsT0 ) return new SignUpError(result.AsT0.Message);

        await _eventDispatcher.PublishAsync(new UserSignedUpEvent
        {
            Timestamp = DateTime.Now,
            UserId = result.AsT1.UserId,
            AuthenticationProvider = result.AsT1.AuthenticationProvider
        }, cancellationToken);
        return Unit.Default;
    }
}
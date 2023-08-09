using Chatify.Application.Authentication.Contracts;
using Chatify.Shared.Abstractions.Commands;
using LanguageExt;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.Authentication.Commands;

using SignOutResult = OneOf<Error, Unit>;

public record SignOut : ICommand<SignOutResult>;

internal sealed class SignOurHandler : ICommandHandler<SignOut, SignOutResult>
{
    private readonly IAuthenticationService _authenticationService;

    public SignOurHandler(IAuthenticationService authenticationService)
        => _authenticationService = authenticationService;

    public async Task<SignOutResult> HandleAsync(
        SignOut command,
        CancellationToken cancellationToken = default)
        => await _authenticationService
            .SignOutAsync(cancellationToken)
            .ToAsync()
            .Match<SignOutResult>(_ => _, err => err);
}
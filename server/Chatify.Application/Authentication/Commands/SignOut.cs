using Chatify.Application.Authentication.Contracts;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Infrastructure.Common.Extensions;
using LanguageExt;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.Authentication.Commands;

using SignOutResult = OneOf<Error, Unit>;

public record SignOut : ICommand<SignOutResult>;

internal sealed class SignOutHandler(IAuthenticationService authenticationService) : ICommandHandler<SignOut, SignOutResult>
{
    public async Task<SignOutResult> HandleAsync(
        SignOut command,
        CancellationToken cancellationToken = default)
    {
        new Identity<int>(10);
        return await authenticationService
            .SignOutAsync(cancellationToken)
            .MatchAsync(err => ( SignOutResult )err, _ => _);
    }
}
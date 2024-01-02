using Chatify.Application.Authentication.Contracts;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Infrastructure.Common.Extensions;
using LanguageExt;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.Authentication.Commands;

using DeclineCookiePolicyResult = OneOf<Error, Unit>;

public record DeclineCookiePolicy : ICommand<DeclineCookiePolicyResult>;

internal sealed class DeclineCookiePolicyHandler(
    IAuthenticationService authenticationService,
    IIdentityContext identityContext
) : ICommandHandler<DeclineCookiePolicy, DeclineCookiePolicyResult>
{
    public async Task<DeclineCookiePolicyResult> HandleAsync(
        DeclineCookiePolicy command,
        CancellationToken cancellationToken = default)
        => await authenticationService.DeclineCookiePolicy(
                identityContext.Id, cancellationToken)
            .MatchAsync<Error, Unit, DeclineCookiePolicyResult>(
                err => err, _ => Unit.Default);
}
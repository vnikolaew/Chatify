using Chatify.Application.Authentication.Contracts;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using LanguageExt;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.Authentication.Commands;

using AcceptCookiePolicyResult = OneOf<Error, Unit>;

public record AcceptCookiePolicy : ICommand<AcceptCookiePolicyResult>;

internal sealed class AcceptCookiePolicyHandler(
        IAuthenticationService authService,
        IIdentityContext identityContext)
    : ICommandHandler<AcceptCookiePolicy, AcceptCookiePolicyResult>
{
    public async Task<AcceptCookiePolicyResult> HandleAsync(
        AcceptCookiePolicy _,
        CancellationToken cancellationToken = default)
    {
        await authService.AcceptCookiePolicy(
            identityContext.Id, cancellationToken);
        return Unit.Default;
    }
}
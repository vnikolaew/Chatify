using Chatify.Application.Authentication.Commands;
using Chatify.Application.Authentication.Contracts;
using Chatify.Application.User.Common;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using LanguageExt;
using OneOf;

namespace Chatify.Application.User.Commands;

using ChangeUserPasswordResult = OneOf<UserNotFound, PasswordChangeError, Unit>;

public record ChangeUserPassword(
    [Password] string OldPassword,
    [Password] string NewPassword
) : ICommand<ChangeUserPasswordResult>;

internal sealed class ChangeUserPasswordHandler
(IIdentityContext identityContext,
    IAuthenticationService authenticationService) : ICommandHandler<ChangeUserPassword,
    ChangeUserPasswordResult>
{
    public async Task<ChangeUserPasswordResult> HandleAsync(
        ChangeUserPassword command,
        CancellationToken cancellationToken = default) =>
        await authenticationService.ChangePasswordAsync(
            identityContext.Id, command.OldPassword, command.NewPassword,
            cancellationToken);
}
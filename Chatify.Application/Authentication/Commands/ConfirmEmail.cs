using System.ComponentModel.DataAnnotations;
using Chatify.Application.Authentication.Contracts;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using LanguageExt;
using LanguageExt.Common;

namespace Chatify.Application.Authentication.Commands;

using ConfirmEmailResult = Either<Error, Unit>;

public record ConfirmEmail([Required] string Token) : ICommand<ConfirmEmailResult>;

internal sealed class ConfirmEmailHandler : ICommandHandler<ConfirmEmail, ConfirmEmailResult>
{
    private readonly IEmailConfirmationService _emailConfirmationService;
    private readonly IIdentityContext _identityContext;

    private Guid UserId => _identityContext.Id;

    public ConfirmEmailHandler(
        IEmailConfirmationService emailConfirmationService,
        IIdentityContext identityContext)
    {
        _emailConfirmationService = emailConfirmationService;
        _identityContext = identityContext;
    }

    public Task<ConfirmEmailResult> HandleAsync(
        ConfirmEmail command,
        CancellationToken cancellationToken = default)
        => _emailConfirmationService
            .ConfirmEmailForUserAsync(command.Token, UserId, cancellationToken);
}
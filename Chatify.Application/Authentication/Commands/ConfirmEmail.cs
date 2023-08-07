using System.ComponentModel.DataAnnotations;
using Chatify.Application.Authentication.Contracts;
using Chatify.Application.Common.Models;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using LanguageExt;
using OneOf;

namespace Chatify.Application.Authentication.Commands;

using ConfirmEmailResult = OneOf<EmailConfirmationError, Unit>;

public record EmailConfirmationError(string Message) : BaseError(Message);

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

    public async Task<ConfirmEmailResult> HandleAsync(
        ConfirmEmail command,
        CancellationToken cancellationToken = default)
    {
        var result = await _emailConfirmationService
            .ConfirmEmailForUserAsync(command.Token, UserId, cancellationToken);
        
        return result.Match<ConfirmEmailResult>(
            _ => new EmailConfirmationError("User not found"),
            _ => _,
            _ => _);
    }
}
using System.ComponentModel.DataAnnotations;
using System.Text;
using Chatify.Application.Authentication.Contracts;
using Chatify.Application.Common.Models;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using LanguageExt;
using OneOf;

namespace Chatify.Application.Authentication.Commands;

using ConfirmEmailResult = OneOf<EmailConfirmationError, Unit>;

public record EmailConfirmationError(string Message) : BaseError(Message);

public record ConfirmEmail([Required] string Token) : ICommand<ConfirmEmailResult>
{
    public const string SuccessMessage = "Email address successfully confirmed.";
};

internal sealed class ConfirmEmailHandler(
        IEmailConfirmationService emailConfirmationService,
        IIdentityContext identityContext)
    : ICommandHandler<ConfirmEmail, ConfirmEmailResult>
{
    private Guid UserId => identityContext.Id;

    public async Task<ConfirmEmailResult> HandleAsync(
        ConfirmEmail command,
        CancellationToken cancellationToken = default)
    {
        var result = await emailConfirmationService
            .ConfirmEmailForUserAsync(command.Token, UserId, cancellationToken);
        
        return result.Match<ConfirmEmailResult>(
            _ => new EmailConfirmationError("User not found"),
            _ => _,
            _ => _);
    }
}
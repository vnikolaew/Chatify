using LanguageExt;
using LanguageExt.Common;

namespace Chatify.Application.Authentication.Contracts;

public interface IEmailConfirmationService
{
    Task<bool> SendConfirmationEmailForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
    
    Task<Either<Error, Unit>> ConfirmEmailForUserAsync(
        string token, Guid userId,
        CancellationToken cancellationToken = default);
}
using Chatify.Application.Authentication.Commands;
using Chatify.Application.User.Commands;
using Chatify.Application.User.Common;
using LanguageExt;
using OneOf;

namespace Chatify.Application.Authentication.Contracts;

public interface IEmailConfirmationService
{
    Task<bool> SendConfirmationEmailForUserAsync(
        Domain.Entities.User user,
        CancellationToken cancellationToken = default);
    
    Task<OneOf<UserNotFound, EmailConfirmationError, Unit>> ConfirmEmailForUserAsync(
        string token, Guid userId,
        CancellationToken cancellationToken = default);
}
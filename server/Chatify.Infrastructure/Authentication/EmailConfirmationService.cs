using System.Text;
using System.Text.Encodings.Web;
using Chatify.Application.Authentication.Commands;
using Chatify.Application.Authentication.Contracts;
using Chatify.Application.Common.Contracts;
using Chatify.Application.User.Common;
using Chatify.Infrastructure.Data.Models;
using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using OneOf;

namespace Chatify.Infrastructure.Authentication;

public sealed class EmailConfirmationService(IServiceScopeFactory scopeFactory,
        IEmailSender emailSender,
        IHttpContextAccessor contextAccessor,
        IAuthenticationService authenticationService)
    : IEmailConfirmationService
{
    private HttpContext HttpContext => contextAccessor.HttpContext!;

    private string GetEmailConfirmationCallbackUrl(string code)
        => new UriBuilder
        {
            Scheme = HttpContext.Request.Scheme,
            Host = HttpContext.Request.Host.Host,
            Path = "auth/confirm-email",
            Query = QueryString.Create("token", code).ToUriComponent(),
        }.ToString();

    public async Task<bool> SendConfirmationEmailForUserAsync(
        Domain.Entities.User user,
        CancellationToken cancellationToken = default)
    {
        // return true;

        var tokenResult = await authenticationService
            .GenerateEmailConfirmationTokenAsync(user.Id, cancellationToken);
        if ( tokenResult.Value is UserNotFound ) return false;

        var token = tokenResult.AsT1!;
        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var callbackUrl = GetEmailConfirmationCallbackUrl(code);

        await emailSender.SendEmailAsync(user.Email, "Confirm your email",
            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

        return true;
    }

    public async Task<OneOf<UserNotFound, EmailConfirmationError, Unit>> ConfirmEmailForUserAsync(
        string token,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ChatifyUser>>();

        var user = await userManager.FindByIdAsync(userId.ToString());
        if ( user is null ) return new UserNotFound();

        var result = await userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded ? Unit.Default : new EmailConfirmationError(result.Errors.First().Description);
    }
}
using System.Text;
using System.Text.Encodings.Web;
using Chatify.Application.Authentication.Contracts;
using Chatify.Application.Common.Contracts;
using Chatify.Infrastructure.Data.Models;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace Chatify.Infrastructure.Authentication;

public sealed class EmailConfirmationService : IEmailConfirmationService
{
    private readonly IEmailSender _emailSender;
    private readonly UserManager<ChatifyUser> _userManager;
    private readonly IHttpContextAccessor _contextAccessor;

    private HttpContext HttpContext => _contextAccessor.HttpContext!;

    public EmailConfirmationService(
        IEmailSender emailSender,
        UserManager<ChatifyUser> userManager,
        IHttpContextAccessor contextAccessor)
    {
        _emailSender = emailSender;
        _userManager = userManager;
        _contextAccessor = contextAccessor;
    }

    public async Task<bool> SendConfirmationEmailForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var builder = new UriBuilder
        {
            Scheme = HttpContext.Request.Scheme,
            Host = HttpContext.Request.Host.Host,
            Path = "auth/confirm-email",
            Query = QueryString.Create("token", code).ToUriComponent(),
        };

        var callbackUrl = builder.ToString();
        await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

        return true;
    }

    public async Task<Either<Error, Unit>> ConfirmEmailForUserAsync(
        string token,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Error.New("User not found.");

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded ? Unit.Default : Error.New("Could not confirm email address.");
    }
}
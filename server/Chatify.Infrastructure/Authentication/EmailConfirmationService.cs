using System.Text;
using System.Text.Encodings.Web;
using Chatify.Application.Authentication.Commands;
using Chatify.Application.Authentication.Contracts;
using Chatify.Application.Common.Contracts;
using Chatify.Application.User.Commands;
using Chatify.Infrastructure.Data.Models;
using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using OneOf;

namespace Chatify.Infrastructure.Authentication;

public sealed class EmailConfirmationService : IEmailConfirmationService
{
    private readonly IEmailSender _emailSender;
    private readonly IAuthenticationService _authenticationService;
    private readonly UserManager<ChatifyUser> _userManager;
    private readonly IHttpContextAccessor _contextAccessor;

    private HttpContext HttpContext => _contextAccessor.HttpContext!;

    public EmailConfirmationService(
        IEmailSender emailSender,
        UserManager<ChatifyUser> userManager,
        IHttpContextAccessor contextAccessor,
        IAuthenticationService authenticationService)
    {
        _emailSender = emailSender;
        _userManager = userManager;
        _contextAccessor = contextAccessor;
        _authenticationService = authenticationService;
    }
    
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
        var tokenResult = await _authenticationService
            .GenerateEmailConfirmationTokenAsync(user.Id, cancellationToken);
        if ( tokenResult.Value is UserNotFound ) return false;

        var token = tokenResult.AsT1!;
        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var callbackUrl = GetEmailConfirmationCallbackUrl(code);
        
        await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

        return true;
    }

    public async Task<OneOf<UserNotFound, EmailConfirmationError, Unit>> ConfirmEmailForUserAsync(
        string token,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if ( user is null ) return new UserNotFound();

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded ? Unit.Default : new EmailConfirmationError(result.Errors.First().Description);
    }
}
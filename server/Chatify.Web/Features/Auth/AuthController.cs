using Chatify.Application.Authentication.Commands;
using Chatify.Infrastructure.Authentication.External.Github;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Common;
using Chatify.Web.Common.Attributes;
using Chatify.Web.Extensions;
using LanguageExt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GithubSignUpResult = OneOf.OneOf<Chatify.Application.Authentication.Models.SignUpError, LanguageExt.Unit>;
using SignOutResult = OneOf.OneOf<LanguageExt.Common.Error, LanguageExt.Unit>;
using RegularSignUpResult = OneOf.OneOf<Chatify.Application.Authentication.Models.SignUpError, LanguageExt.Unit>;
using RegularSignInResult = OneOf.OneOf<Chatify.Application.Authentication.Models.SignInError, LanguageExt.Unit>;
using GoogleSignUpResult = OneOf.OneOf<Chatify.Application.Authentication.Models.SignUpError, LanguageExt.Unit>;
using FacebookSignUpResult = OneOf.OneOf<Chatify.Application.Authentication.Models.SignUpError, LanguageExt.Unit>;
using ConfirmEmailResult =
    OneOf.OneOf<Chatify.Application.Authentication.Commands.EmailConfirmationError, LanguageExt.Unit>;
using AcceptCookiePolicyResult = OneOf.OneOf<LanguageExt.Common.Error, LanguageExt.Unit>;
using DeclineCookiePolicyResult = OneOf.OneOf<LanguageExt.Common.Error, LanguageExt.Unit>;

namespace Chatify.Web.Features.Auth;

[AllowAnonymous]
public class AuthController : ApiController
{
    public const string RegularSignUpRoute = "signup";
    public const string SignOutRoute = "signout";
    public const string RegularSignInRoute = "signin";

    public const string GoogleSignUpRoute = $"{RegularSignUpRoute}/google";
    public const string FacebookSignUpRoute = $"{RegularSignUpRoute}/facebook";
    public const string GithubSignUpRoute = $"{RegularSignUpRoute}/github";

    public const string ConfirmEmailRoute = "confirm-email";
    public const string CookiePolicyRoute = "cookie-policy";

    [HttpGet]
    [Authorize]
    [Route("me")]
    [ProducesOkApiResponse<object>]
    public IActionResult Info()
        => Ok(new
        {
            Claims = User.Identity?.IsAuthenticated ?? false
                ? User.Claims
                    .DistinctBy(c => c.Type)
                    .ToDictionary(
                        c => c.Type.Split("/", StringSplitOptions.RemoveEmptyEntries).Last(),
                        c => c.Value)
                : null!
        });

    [HttpPost]
    [Route(RegularSignUpRoute)]
    [ProducesBadRequestApiResponse]
    [ProducesNoContentApiResponse]
    public Task<IActionResult> RegularSignUp(
        [FromBody] RegularSignUp signUp,
        CancellationToken cancellationToken = default)
        => SendAsync<RegularSignUp, RegularSignUpResult>(signUp, cancellationToken)
            .MatchAsync(err => err.ToBadRequest(), NoContent);

    [HttpPost]
    [Route(SignOutRoute)]
    [ProducesBadRequestApiResponse]
    [ProducesNoContentApiResponse]
    public Task<IActionResult> SignOut(CancellationToken cancellationToken = default)
        => SendAsync<SignOut, SignOutResult>(new SignOut(), cancellationToken)
            .MatchAsync(err => err.ToBadRequest(), NoContent);

    [HttpPost]
    [Route(RegularSignInRoute)]
    [ProducesBadRequestApiResponse]
    [ProducesNoContentApiResponse]
    [ProducesRedirectApiResponse]
    public Task<IActionResult> RegularSignIn(
        [FromBody] RegularSignIn signIn,
        [FromQuery] string? returnUrl,
        CancellationToken cancellationToken = default)
    {
        return SendAsync<RegularSignIn, RegularSignInResult>(signIn, cancellationToken)
            .MatchAsync(
                err => err.ToBadRequest(),
                _ => returnUrl is not null
                    ? RedirectToUrl(returnUrl)
                    : NoContent());
    }

    private IActionResult RedirectToUrl(string returnUrl)
        => Url.IsLocalUrl(returnUrl) switch
        {
            true => Redirect($"{Request.Host.Host}/{returnUrl}"),
            _ => Redirect(returnUrl)
        };

    [HttpPost]
    [Route(GoogleSignUpRoute)]
    [ProducesBadRequestApiResponse]
    [ProducesNoContentApiResponse]
    public Task<IActionResult> GoogleSignUp(
        [FromBody] GoogleSignUp signUp,
        [FromQuery] string? returnUrl,
        CancellationToken cancellationToken = default)
        => SendAsync<GoogleSignUp, GoogleSignUpResult>(signUp, cancellationToken)
            .MatchAsync(err => err.ToBadRequest(),
                _ => returnUrl is not null
                    ? RedirectToUrl(returnUrl)
                    : NoContent());

    [HttpPost]
    [Route(FacebookSignUpRoute)]
    [ProducesBadRequestApiResponse]
    [ProducesNoContentApiResponse]
    public Task<IActionResult> FacebookSignUp(
        [FromBody] FacebookSignUp signUp,
        CancellationToken cancellationToken = default)
        => SendAsync<FacebookSignUp, FacebookSignUpResult>(signUp, cancellationToken)
            .MatchAsync(err => err.ToBadRequest(), NoContent);

    [HttpPost]
    [Route(GithubSignUpRoute)]
    [ProducesBadRequestApiResponse]
    [ProducesNoContentApiResponse]
    public Task<IActionResult> GithubSignUp(
        [FromServices] IGithubOAuthClient githubOAuthClient,
        [FromQuery] string code,
        [FromQuery] string? returnUrl,
        CancellationToken cancellationToken = default)
        => SendAsync<GithubSignUp, GithubSignUpResult>(
                new GithubSignUp(code), cancellationToken)
            .MatchAsync(
                err => err.ToBadRequest(),
                _ => returnUrl is not null
                    ? RedirectToUrl(returnUrl)
                    : NoContent());

    [HttpPost]
    [Route(ConfirmEmailRoute)]
    [Authorize]
    [ProducesBadRequestApiResponse]
    [ProducesAcceptedApiResponse<ApiResponse<Unit>>]
    public Task<IActionResult> ConfirmEmail(
        [FromQuery(Name = "token")] string tokenCode,
        CancellationToken cancellationToken = default)
        => SendAsync<ConfirmEmail, ConfirmEmailResult>(
                new ConfirmEmail(tokenCode), cancellationToken)
            .MatchAsync(
                err => err.ToBadRequest(),
                _ => Accepted(Application.Authentication.Commands.ConfirmEmail.SuccessMessage));

    [HttpPost]
    [Route(CookiePolicyRoute)]
    [ProducesBadRequestApiResponse]
    [ProducesAcceptedApiResponse]
    public Task<IActionResult> AcceptCookiePolicy(
        CancellationToken cancellationToken = default)
        => SendAsync<AcceptCookiePolicy, AcceptCookiePolicyResult>(
                new AcceptCookiePolicy(), cancellationToken)
            .MatchAsync(
                err => err.ToBadRequest(),
                Accepted);

    [HttpDelete]
    [Route(CookiePolicyRoute)]
    [ProducesBadRequestApiResponse]
    [ProducesAcceptedApiResponse]
    public Task<IActionResult> DeclineCookiePolicy(
        CancellationToken cancellationToken = default)
        => SendAsync<DeclineCookiePolicy, DeclineCookiePolicyResult>(
                new DeclineCookiePolicy(), cancellationToken)
            .MatchAsync(
                err => err.ToBadRequest(),
                Accepted);
}
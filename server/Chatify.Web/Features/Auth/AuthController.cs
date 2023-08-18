using Chatify.Application.Authentication.Commands;
using Chatify.Infrastructure.Authentication.External.Github;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Common;
using Chatify.Web.Extensions;
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

    [HttpGet]
    [Authorize]
    [Route("me")]
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
    public Task<IActionResult> RegularSignUp(
        [FromBody] RegularSignUp signUp,
        CancellationToken cancellationToken = default)
        => SendAsync<RegularSignUp, RegularSignUpResult>(signUp, cancellationToken)
            .MatchAsync(err => err.ToBadRequest(), NoContent);

    [HttpPost]
    [Route(SignOutRoute)]
    public Task<IActionResult> SignOut(CancellationToken cancellationToken = default)
        => SendAsync<SignOut, SignOutResult>(new SignOut(), cancellationToken)
            .MatchAsync(err => err.ToBadRequest(), NoContent);

    [HttpPost]
    [Route(RegularSignInRoute)]
    public Task<IActionResult> RegularSignIn(
        [FromBody] RegularSignIn signIn,
        [FromQuery] string? returnUrl,
        CancellationToken cancellationToken = default)
    {
        return SendAsync<RegularSignIn, RegularSignInResult>(signIn, cancellationToken)
            .MatchAsync(
                err => err.ToBadRequest(),
                _ => returnUrl is not null
                    ? Redirect(returnUrl)
                    : NoContent());
    }

    [HttpPost]
    [Route(GoogleSignUpRoute)]
    public Task<IActionResult> GoogleSignUp(
        [FromBody] GoogleSignUp signUp,
        CancellationToken cancellationToken = default)
    {
        return SendAsync<GoogleSignUp, GoogleSignUpResult>(signUp, cancellationToken)
            .MatchAsync(err => err.ToBadRequest(), NoContent);
    }

    [HttpPost]
    [Route(FacebookSignUpRoute)]
    public Task<IActionResult> FacebookSignUp(
        [FromBody] FacebookSignUp signUp,
        CancellationToken cancellationToken = default)
    {
        return SendAsync<FacebookSignUp, FacebookSignUpResult>(signUp, cancellationToken)
            .MatchAsync(err => err.ToBadRequest(), NoContent);
    }

    [HttpPost]
    [Route(GithubSignUpRoute)]
    public Task<IActionResult> GithubSignUp(
        [FromServices] IGithubOAuthClient githubOAuthClient,
        [FromQuery] string code,
        CancellationToken cancellationToken = default)
        => SendAsync<GithubSignUp, GithubSignUpResult>(
                new GithubSignUp(code), cancellationToken)
            .MatchAsync(err => err.ToBadRequest(), _ => NoContent());

    [HttpPost]
    [Route(ConfirmEmailRoute)]
    public Task<IActionResult> ConfirmEmail(
        [FromQuery(Name = "token")] string tokenCode,
        CancellationToken cancellationToken = default)
    {
        return SendAsync<ConfirmEmail, ConfirmEmailResult>(
                new ConfirmEmail(tokenCode), cancellationToken)
            .MatchAsync(err => err.ToBadRequest(), Accepted);
    }
}
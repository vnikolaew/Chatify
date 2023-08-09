using Chatify.Application.Authentication.Commands;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Common;
using Chatify.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    private const string RegularSignUpRoute = "signup";
    private const string SignOutRoute = "signout";
    private const string RegularSignInRoute = "signin";

    private const string GoogleSignUpRoute = $"{RegularSignUpRoute}/google";
    private const string FacebookSignUpRoute = $"{RegularSignUpRoute}/facebook";

    private const string ConfirmEmailRoute = "confirm-email";

    [HttpGet]
    [Route("me")]
    public IActionResult Info()
        => Ok(new
        {
            Claims = User.Claims.ToDictionary(c => c.Type, c => c.Value)
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
        CancellationToken cancellationToken = default)
    {
        return SendAsync<RegularSignIn, RegularSignInResult>(signIn, cancellationToken)
            .MatchAsync(err => err.ToBadRequest(), NoContent);
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
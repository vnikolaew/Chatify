using Chatify.Application.Authentication.Commands;
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
    public async Task<IActionResult> RegularSignUp(
        [FromBody] RegularSignUp signUp,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<RegularSignUp, RegularSignUpResult>(signUp, cancellationToken);
        return result.Match<IActionResult>(
            err => err.ToBadRequest(),
            NoContent);
    }

    [HttpPost]
    [Route(SignOutRoute)]
    public async Task<IActionResult> SignOut(CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<SignOut, SignOutResult>(new SignOut(), cancellationToken);
        return result.Match<IActionResult>(
            err => err.ToBadRequest(),
            NoContent);
    }

    [HttpPost]
    [Route(RegularSignInRoute)]
    public async Task<IActionResult> RegularSignIn(
        [FromBody] RegularSignIn signIn,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<RegularSignIn, RegularSignInResult>(signIn, cancellationToken);
        return result.Match<IActionResult>(
            err => err.ToBadRequest(),
            NoContent);
    }

    [HttpPost]
    [Route(GoogleSignUpRoute)]
    public async Task<IActionResult> GoogleSignUp(
        [FromBody] GoogleSignUp signUp,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<GoogleSignUp, GoogleSignUpResult>(signUp, cancellationToken);
        return result.Match<IActionResult>(
            err => err.ToBadRequest(),
            NoContent);
    }

    [HttpPost]
    [Route(FacebookSignUpRoute)]
    public async Task<IActionResult> FacebookSignUp(
        [FromBody] FacebookSignUp signUp,
        CancellationToken cancellationToken = default)
    {
        var result = await
            SendAsync<FacebookSignUp, FacebookSignUpResult>(signUp, cancellationToken);

        return result.Match<IActionResult>(
            err => err.ToBadRequest(),
            NoContent);
    }

    [HttpPost]
    [Route(ConfirmEmailRoute)]
    public async Task<IActionResult> ConfirmEmail(
        [FromQuery(Name = "token")] string tokenCode,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<ConfirmEmail, ConfirmEmailResult>(
            new ConfirmEmail(tokenCode), cancellationToken);
        return result.Match<IActionResult>(
            err => err.ToBadRequest(),
            Accepted);
    }
}
using Chatify.Application.Authentication.Commands;
using Chatify.Web.Common;
using Chatify.Web.Extensions;
using LanguageExt.SomeHelp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegularSignUpResult = LanguageExt.Validation<LanguageExt.Common.Error, LanguageExt.Unit>;
using RegularSignInResult = LanguageExt.Validation<LanguageExt.Common.Error, LanguageExt.Unit>;
using GoogleSignUpResult = LanguageExt.Validation<LanguageExt.Common.Error, LanguageExt.Unit>;
using FacebookSignUpResult = LanguageExt.Validation<LanguageExt.Common.Error, LanguageExt.Unit>;
using ConfirmEmailResult = LanguageExt.Either<LanguageExt.Common.Error, LanguageExt.Unit>;

namespace Chatify.Web.Features.Auth;

[AllowAnonymous]
public class AuthController : ApiController
{
    private const string RegularSignUpRoute = "signup";
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
            _ => NoContent(),
            err => err.ToBadRequest());
    }

    [HttpPost]
    [Route(RegularSignInRoute)]
    public async Task<IActionResult> RegularSignIn(
        [FromBody] RegularSignIn signIn,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<RegularSignIn, RegularSignInResult>(signIn, cancellationToken);
        return result.Match<IActionResult>(
            _ => NoContent(),
            err => err.ToBadRequest());
    }

    [HttpPost]
    [Route(GoogleSignUpRoute)]
    public async Task<IActionResult> GoogleSignUp(
        [FromBody] GoogleSignUp signUp,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<GoogleSignUp, GoogleSignUpResult>(signUp, cancellationToken);
        return result.Match<IActionResult>(
            _ => NoContent(),
            err => err.ToBadRequest());
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
            _ => NoContent(),
            err => err.ToBadRequest());
    }

    [HttpPost]
    [Route(ConfirmEmailRoute)]
    public async Task<IActionResult> ConfirmEmail(
        [FromBody] ConfirmEmail confirmEmail,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<ConfirmEmail, ConfirmEmailResult>(confirmEmail, cancellationToken);
        return result.Match<IActionResult>(
            _ => Accepted(),
            err => err.ToBadRequest());
    }
}
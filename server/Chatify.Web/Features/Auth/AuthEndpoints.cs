using Chatify.Application.Authentication.Commands;
using Chatify.Shared.Abstractions.Dispatchers;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Chatify.Web.Features.Auth;

using RegularSignUpResult = OneOf.OneOf<Application.Authentication.Models.SignUpError, LanguageExt.Unit>;
using SignOutResult = OneOf.OneOf<LanguageExt.Common.Error, LanguageExt.Unit>;
using RegularSignInResult = OneOf.OneOf<Application.Authentication.Models.SignInError, LanguageExt.Unit>;

internal static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuth(
        this IEndpointRouteBuilder routeBuilder)
    {
        var groupRouteBuilder = routeBuilder
            .MapGroup("/api/auth")
            .AllowAnonymous();

        groupRouteBuilder.MapPost(AuthController.RegularSignUpRoute, RegularSignUp);
        groupRouteBuilder.MapPost(AuthController.SignOutRoute, SignOut);
        groupRouteBuilder.MapPost(AuthController.RegularSignInRoute, SignOut);
        groupRouteBuilder.MapPost(AuthController.RegularSignInRoute, RegularSignIn);

        return routeBuilder;
    }

    private static Task<IResult> RegularSignUp(
        [FromBody] RegularSignUp signUp,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
        => dispatcher
            .SendAsync<RegularSignUp, RegularSignUpResult>(signUp, cancellationToken)
            .MatchAsync(
                err => err.ToBadRequestResult(),
                _ => TypedResults.NoContent());

    private static Task<IResult> SignOut(
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
        => dispatcher
            .SendAsync<SignOut, SignOutResult>(new SignOut(), cancellationToken)
            .MatchAsync(
                err => err.ToBadRequestResult(),
                _ => TypedResults.NoContent());

    private static Task<IResult> RegularSignIn(
        [FromBody] RegularSignIn signIn,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
        => dispatcher
            .SendAsync<RegularSignIn, RegularSignInResult>(signIn, cancellationToken)
            .MatchAsync(
                err => err.ToBadRequestResult(),
                _ => TypedResults.NoContent());
}
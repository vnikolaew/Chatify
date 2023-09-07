using Chatify.Application.Authentication.Commands;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Extensions;
using Chatify.Web.FastEndpoints_Features.Common;
using FastEndpoints;
using Microsoft.AspNetCore.Mvc;

namespace Chatify.Web.FastEndpoints_Features.Auth;

using GithubSignUpResult = OneOf.OneOf<Application.Authentication.Models.SignUpError, LanguageExt.Unit>;

[FastEndpoints.HttpPost("signup/github")]
public sealed class GithubSignUpEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IUrlHelper _url;

    public GithubSignUpEndpoint(IUrlHelper url)
    {
        _url = url;
        AllowAnonymous();
    }

    public override Task<IResult> HandleAsync(
        EmptyRequest req,
        CancellationToken ct)
    {
        var code = Query<string>("code", isRequired: false);
        var returnUrl = Query<string>("returnUrl", isRequired: false);

        return SendAsync<GithubSignUp, GithubSignUpResult>(
                new GithubSignUp(code), ct)
            .MatchAsync(
                err => err.ToBadRequestResult(),
                _ => returnUrl is not null
                    ? _url.IsLocalUrl(returnUrl) switch
                    {
                        true => ( IResult )TypedResults.Redirect($"{HttpContext.Request.Host.Host}/${returnUrl}"),
                        _ => TypedResults.Redirect(returnUrl)
                    }
                    : TypedResults.NoContent());
    }
}
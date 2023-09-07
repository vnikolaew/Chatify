using Chatify.Application.Authentication.Commands;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Extensions;
using Chatify.Web.FastEndpoints_Features.Common;
using Microsoft.AspNetCore.Mvc;

namespace Chatify.Web.FastEndpoints_Features.Auth;

using GoogleSignUpResult = OneOf.OneOf<Application.Authentication.Models.SignUpError, LanguageExt.Unit>;

[FastEndpoints.HttpPost("signup/google")]
public sealed class GoogleSignUpEndpoint : BaseEndpoint<GoogleSignUp, IResult>
{
    private readonly IUrlHelper _url;

    public GoogleSignUpEndpoint(IUrlHelper url)
    {
        _url = url;
        AllowAnonymous();
    }

    public override Task<IResult> HandleAsync(
        GoogleSignUp req,
        CancellationToken ct)
    {
        var returnUrl = Query<string>("returnUrl", isRequired: false);

        return SendAsync<GoogleSignUp, GoogleSignUpResult>(req, ct)
            .MatchAsync(err => err.ToBadRequestResult(),
                _ => returnUrl is not null
                    ? _url.IsLocalUrl(returnUrl) switch
                    {
                        true => ( IResult )TypedResults.Redirect($"{HttpContext.Request.Host.Host}/${returnUrl}"),
                        _ => TypedResults.Redirect(returnUrl)
                    }
                    : TypedResults.NoContent());
    }
}
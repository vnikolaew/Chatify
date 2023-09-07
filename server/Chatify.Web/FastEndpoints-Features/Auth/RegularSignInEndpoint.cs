﻿using Chatify.Application.Authentication.Commands;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Extensions;
using Chatify.Web.FastEndpoints_Features.Common;
using Microsoft.AspNetCore.Mvc;

namespace Chatify.Web.FastEndpoints_Features.Auth;

using RegularSignInResult = OneOf.OneOf<Application.Authentication.Models.SignInError, LanguageExt.Unit>;

[FastEndpoints.HttpPost("signin")]
public class RegularSignInEndpoint : BaseEndpoint<RegularSignIn, IResult>
{
    private readonly IUrlHelper _url;

    public RegularSignInEndpoint(IUrlHelper url)
    {
        _url = url;
        AllowAnonymous();
    }

    public override Task<IResult> HandleAsync(
        RegularSignIn req,
        CancellationToken ct)
    {
        var returnUrl = Query<string>("returnUrl", isRequired: false);

        return SendAsync<RegularSignIn, RegularSignInResult>(req, ct)
            .MatchAsync(
                err => err.ToBadRequestResult(),
                _ => returnUrl is not null
                    ? _url.IsLocalUrl(returnUrl) switch
                    {
                        true => ( IResult )TypedResults.Redirect(GetRedirectUrl(returnUrl)),
                        _ => TypedResults.Redirect(returnUrl)
                    }
                    : TypedResults.NoContent());
    }

    private string GetRedirectUrl(string returnUrl)
        => $"{HttpContext.Request.Host.Host}/{returnUrl}";
}
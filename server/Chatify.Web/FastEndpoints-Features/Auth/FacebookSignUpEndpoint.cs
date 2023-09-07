using Chatify.Application.Authentication.Commands;
using Chatify.Web.FastEndpoints_Features.Common;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Extensions;
using FastEndpoints;

namespace Chatify.Web.FastEndpoints_Features.Auth;

using FacebookSignUpResult = OneOf.OneOf<Application.Authentication.Models.SignUpError, LanguageExt.Unit>;

[HttpPost("signup/facebook")]
public sealed class FacebookSignUpEndpoint : BaseEndpoint<FacebookSignUp, IResult>
{
    public FacebookSignUpEndpoint() => AllowAnonymous();

    public override Task<IResult> HandleAsync(
        FacebookSignUp req,
        CancellationToken ct)
        => SendAsync<FacebookSignUp, FacebookSignUpResult>(req, ct)
            .MatchAsync(err => err.ToBadRequestResult(),
                _ => ( IResult )TypedResults.NoContent());
}
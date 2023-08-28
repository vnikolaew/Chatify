using Chatify.Application.Authentication.Commands;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Extensions;
using Chatify.Web.FastEndpoints_Features.Common;
using FastEndpoints;

using RegularSignUpResult = OneOf.OneOf<Chatify.Application.Authentication.Models.SignUpError, LanguageExt.Unit>;
using RegularSignUpResponse =
    Microsoft.AspNetCore.Http.HttpResults.Results<Microsoft.AspNetCore.Http.HttpResults.BadRequest<object>,
        Microsoft.AspNetCore.Http.HttpResults.NoContent>;

namespace Chatify.Web.FastEndpoints_Features;

[HttpPost("signup")]
public sealed class RegularSignUpEndpoint
    : BaseEndpoint<RegularSignUp, RegularSignUpResponse>
{
    public RegularSignUpEndpoint()
        => AllowAnonymous();

    public override Task<RegularSignUpResponse> HandleAsync(
        RegularSignUp request,
        CancellationToken ct)
        => SendAsync<RegularSignUp, RegularSignUpResult>(request, ct)
            .MatchAsync(
                err => ( RegularSignUpResponse )
                    err.ToBadRequestResult(),
                _ => TypedResults.NoContent());
}
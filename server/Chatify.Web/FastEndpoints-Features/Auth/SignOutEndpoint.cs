using Chatify.Application.Authentication.Commands;
using Chatify.Web.FastEndpoints_Features.Common;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Extensions;
using FastEndpoints;
using SignOutResult = OneOf.OneOf<LanguageExt.Common.Error, LanguageExt.Unit>;
using SignOutResponse =
    Microsoft.AspNetCore.Http.HttpResults.Results<Microsoft.AspNetCore.Http.HttpResults.BadRequest,
        Microsoft.AspNetCore.Http.HttpResults.NoContent>;

namespace Chatify.Web.FastEndpoints_Features.Auth;

[HttpPost("signout")]
public class SignOutEndpoint : BaseEndpoint<EmptyRequest, SignOutResponse>
{
    public override Task<SignOutResponse> HandleAsync(EmptyRequest req,
        CancellationToken ct)
        => SendAsync<SignOut, SignOutResult>(new SignOut(), ct)
            .MatchAsync(err => ( SignOutResponse )err.ToBadRequestResult(),
                _ => TypedResults.NoContent());
}
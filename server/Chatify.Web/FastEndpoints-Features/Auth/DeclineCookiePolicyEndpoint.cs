using Chatify.Application.Authentication.Commands;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Extensions;
using Chatify.Web.FastEndpoints_Features.Common;
using FastEndpoints;
using DeclineCookiePolicyResult = OneOf.OneOf<LanguageExt.Common.Error, LanguageExt.Unit>;

namespace Chatify.Web.FastEndpoints_Features.Auth;

[HttpDelete("cookie-policy")]
public class DeclineCookiePolicyEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    public override Task<IResult> HandleAsync(EmptyRequest req,
        CancellationToken ct)
        => SendAsync<DeclineCookiePolicy, DeclineCookiePolicyResult>(
                new DeclineCookiePolicy(), ct)
            .MatchAsync(
                err => err.ToBadRequestResult(),
                Accepted);
}
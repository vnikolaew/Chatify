using Chatify.Application.Authentication.Commands;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Extensions;
using Chatify.Web.FastEndpoints_Features.Common;
using FastEndpoints;

using AcceptCookiePolicyResult = OneOf.OneOf<LanguageExt.Common.Error, LanguageExt.Unit>;

namespace Chatify.Web.FastEndpoints_Features.Auth;

[HttpPost("cookie-policy")]
public sealed class AcceptCookiePolicyEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    public override Task<IResult> HandleAsync(EmptyRequest req,
        CancellationToken ct)
        => SendAsync<AcceptCookiePolicy, AcceptCookiePolicyResult>(
                new AcceptCookiePolicy(), ct)
            .MatchAsync(
                err => err.ToBadRequestResult(),
                Accepted);
}
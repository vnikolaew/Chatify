using Chatify.Application.Authentication.Commands;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Extensions;
using Chatify.Web.FastEndpoints_Features.Common;
using FastEndpoints;

using ConfirmEmailResult =
    OneOf.OneOf<Chatify.Application.Authentication.Commands.EmailConfirmationError, LanguageExt.Unit>;

namespace Chatify.Web.FastEndpoints_Features.Auth;

[HttpPost("confirm-email")]
public sealed class ConfirmEmailEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    public override Task<IResult> HandleAsync(EmptyRequest req,
        CancellationToken ct)
    {
        var token = Query<string>("token");
        
        return SendAsync<ConfirmEmail, ConfirmEmailResult>(
                new ConfirmEmail(token), ct)
            .MatchAsync(
                err => err.ToBadRequestResult(),
                _ => Accepted(ConfirmEmail.SuccessMessage));
    }
}
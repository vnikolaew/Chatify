using Chatify.Application.User.Commands;
using Chatify.Web.Common;
using Chatify.Web.Extensions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;

namespace Chatify.Web.Features.Profile;

using ChangeUserStatusResult = Either<Error, Unit>;
using ChangeUserDetailsResult = Either<Error, Unit>;

public class ProfileController : ApiController
{
    [HttpPut]
    [Route("status")]
    public Task<IActionResult> ChangeUserStatus(
        [FromBody] ChangeUserStatus request,
        CancellationToken cancellationToken = default
    )
        => SendAsync<ChangeUserStatus, ChangeUserStatusResult>(request, cancellationToken)
            .ToAsync()
            .Match(_ => Accepted(), err => err.ToBadRequest());

    [HttpPatch]
    [Route("details")]
    public Task<IActionResult> ChangeUserDetails(
        [FromBody] EditUserDetails request,
        CancellationToken cancellationToken = default)
        => SendAsync<EditUserDetails, ChangeUserDetailsResult>(request, cancellationToken)
            .ToAsync()
            .Match(_ => Accepted(), err => err.ToBadRequest());
}
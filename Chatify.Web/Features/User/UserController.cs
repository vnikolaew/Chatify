using Chatify.Application.User.Commands;
using Chatify.Web.Common;
using Chatify.Web.Extensions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;

namespace Chatify.Web.Features.User;

using ChangeUserStatusResult = Either<Error, Unit>;

public class UserController : ApiController
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
}
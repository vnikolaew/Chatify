using System.Net;
using Chatify.Application.User.Commands;
using Chatify.Application.User.Common;
using Chatify.Application.User.Queries;
using Chatify.Domain.Entities;
using Chatify.Web.Common;
using Chatify.Web.Common.Attributes;
using Chatify.Web.Extensions;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using OneOf;

namespace Chatify.Web.Features.Profile;


using ChangeUserStatusResult = OneOf<UserNotFound, Unit>;
using GetUserDetailsResult = OneOf<UserNotFound, NotFriendsError, UserDetailsEntry>;
using EditUserDetailsResult =
    OneOf<UserNotFound, FileUploadError, PasswordChangeError, Unit>;
using SearchUsersByNameResult = OneOf<UserNotFound, List<User>>;

public class ProfileController : ApiController
{
    [HttpPut]
    [Route("status")]
    [ProducesBadRequestApiResponse]
    [ProducesAcceptedApiResponse]
    public async Task<IActionResult> ChangeUserStatus(
        [FromBody] ChangeUserStatus request,
        CancellationToken cancellationToken = default
    )
    {
        var result = await SendAsync<ChangeUserStatus, ChangeUserStatusResult>(request, cancellationToken);
        return result.Match<IActionResult>(
            _ => NotFound(),
            Accepted);
    }

    [HttpPatch]
    [Route("details")]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesAcceptedApiResponse]
    public async Task<IActionResult> ChangeUserDetails(
        [FromBody] EditUserDetails request,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<EditUserDetails, EditUserDetailsResult>(request, cancellationToken);

        return result.Match<IActionResult>(
            _ => NotFound(),
            _ => _.ToBadRequest(),
            _ => _.ToBadRequest(),
            Accepted);
    }

    [HttpGet]
    [Route("{userId:guid}/details")]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesOkApiResponse<UserDetailsEntry>]
    public async Task<IActionResult> GetUserDetails(
        [FromRoute] Guid userId,
        CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<GetUserDetails, GetUserDetailsResult>(
            new GetUserDetails(userId), cancellationToken);
        return result.Match<IActionResult>(
            _ => NotFound(),
            _ => _.ToBadRequest(),
            Ok);
    }

    [HttpGet]
    [Route("search")]
    [ProducesNotFoundApiResponse]
    [ProducesOkApiResponse<List<User>>]
    public async Task<IActionResult> SearchUsersByName(
        [FromQuery] string usernameQuery,
        CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<SearchUsersByName, SearchUsersByNameResult>(
            new SearchUsersByName(usernameQuery),
            cancellationToken);

        return result.Match<IActionResult>(
            _ => NotFound(),
            Ok);
    }
}
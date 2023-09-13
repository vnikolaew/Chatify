using Chatify.Application.User.Commands;
using Chatify.Application.User.Common;
using Chatify.Application.User.Queries;
using Chatify.Domain.Entities;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Common;
using Chatify.Web.Common.Attributes;
using Chatify.Web.Extensions;
using Chatify.Web.Features.Profile.Models;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using OneOf;

namespace Chatify.Web.Features.Profile;

using ChangeUserStatusResult = OneOf<UserNotFound, Unit>;
using ChangeUserPasswordResult = OneOf<UserNotFound, PasswordChangeError, Unit>;
using GetUserDetailsResult = OneOf<UserNotFound, NotFriendsError, UserDetailsEntry>;
using EditUserDetailsResult =
    OneOf<UserNotFound, FileUploadError, PasswordChangeError, Unit>;
using SearchUsersByNameResult = OneOf<UserNotFound, List<User>>;
using FindByUserHandleResult = OneOf<UserNotFound, UserDetailsEntry>;

public class ProfileController : ApiController
{
    [HttpPut]
    [Route("status")]
    [ProducesBadRequestApiResponse]
    [ProducesAcceptedApiResponse]
    public Task<IActionResult> ChangeUserStatus(
        [FromBody] ChangeUserStatus request,
        CancellationToken cancellationToken = default
    ) => SendAsync<ChangeUserStatus, ChangeUserStatusResult>(
            request, cancellationToken)
        .MatchAsync(
            _ => NotFound(),
            Accepted);

    [HttpPatch]
    [Route("details")]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesAcceptedApiResponse]
    public async Task<IActionResult> ChangeUserDetails(
        [FromForm] Requests.EditUserDetailsRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<EditUserDetails, EditUserDetailsResult>(
            request.ToCommand(), cancellationToken);

        return result.Match<IActionResult>(
            _ => NotFound(),
            _ => _.ToBadRequest(),
            _ => _.ToBadRequest(),
            Accepted);
    }

    [HttpPut]
    [Route("password")]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesAcceptedApiResponse]
    public async Task<IActionResult> ChangeUserPassword(
        [FromBody] ChangeUserPassword request,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<ChangeUserPassword, ChangeUserPasswordResult>(
            request, cancellationToken);

        return result.Match<IActionResult>(
            _ => NotFound(),
            _ => _.ToBadRequest(),
            Accepted);
    }

    [HttpGet]
    [Route("{userId:guid}/details")]
    [ProducesBadRequestApiResponse]
    [ProducesNotFoundApiResponse]
    [ProducesOkApiResponse<UserDetailsEntry>]
    public Task<IActionResult> GetUserDetails(
        [FromRoute] Guid userId,
        CancellationToken cancellationToken = default)
        => QueryAsync<GetUserDetails, GetUserDetailsResult>(
                new GetUserDetails(userId), cancellationToken)
            .MatchAsync(
                _ => NotFound(),
                _ => _.ToBadRequest(),
                Ok);

    [HttpGet]
    [Route("search")]
    [ProducesNotFoundApiResponse]
    [ProducesOkApiResponse<List<User>>]
    public Task<IActionResult> SearchUsersByName(
        [FromQuery] string usernameQuery,
        CancellationToken cancellationToken = default)
        => QueryAsync<SearchUsersByName, SearchUsersByNameResult>(
                new SearchUsersByName(usernameQuery),
                cancellationToken)
            .MatchAsync(
                _ => NotFound(),
                Ok);

    [HttpGet]
    [ProducesNotFoundApiResponse]
    [ProducesOkApiResponse<UserDetailsEntry>]
    public Task<IActionResult> FindByHandle(
        [FromQuery(Name = "handle")] string userHandle,
        CancellationToken cancellationToken = default)
        => QueryAsync<FindByUserHandle, FindByUserHandleResult>(
                new FindByUserHandle(userHandle),
                cancellationToken)
            .MatchAsync(_ => NotFound(), Ok);
}
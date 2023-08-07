﻿using Chatify.Application.User.Commands;
using Chatify.Application.User.Queries;
using Chatify.Web.Common;
using Chatify.Web.Extensions;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using UserNotFound = Chatify.Application.User.Queries.UserNotFound;

namespace Chatify.Web.Features.Profile;

using ChangeUserStatusResult = OneOf.OneOf<UserNotFound, Unit>;
using GetUserDetailsResult = OneOf.OneOf<UserNotFound, NotFriendsError, Domain.Entities.User>;
using EditUserDetailsResult =
    OneOf.OneOf<Chatify.Application.User.Commands.UserNotFound, FileUploadError, PasswordChangeError, Unit>;
using SearchUsersByNameResult = OneOf.OneOf<UserNotFound, Domain.Entities.User>;

public class ProfileController : ApiController
{
    [HttpPut]
    [Route("status")]
    public async Task<IActionResult> ChangeUserStatus(
        [FromBody] ChangeUserStatus request,
        CancellationToken cancellationToken = default
    )
    {
        var result = await SendAsync<ChangeUserStatus, ChangeUserStatusResult>(request, cancellationToken);
        return result.Match<IActionResult>(
            _ => BadRequest(),
            _ => Accepted());
    }

    [HttpPatch]
    [Route("details")]
    public async Task<IActionResult> ChangeUserDetails(
        [FromBody] EditUserDetails request,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<EditUserDetails, EditUserDetailsResult>(request, cancellationToken);

        return result.Match<IActionResult>(
            _ => BadRequest(),
            _ => _.ToBadRequest(),
            _ => _.ToBadRequest(),
            _ => Accepted());
    }

    [HttpGet]
    [Route("{userId:guid}/details")]
    public async Task<IActionResult> GetUserDetails(
        [FromRoute] Guid userId,
        CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<GetUserDetails, GetUserDetailsResult>(
            new GetUserDetails(userId), cancellationToken);
        return result.Match<IActionResult>(
            _ => BadRequest(),
            _ => _.ToBadRequest(),
            Ok);
    }

    [HttpGet]
    [Route("details")]
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
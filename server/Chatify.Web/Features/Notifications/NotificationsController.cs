using Chatify.Application.Notifications.Commands;
using Chatify.Application.Notifications.Queries;
using Chatify.Domain.Entities;
using Chatify.Shared.Abstractions.Queries;
using Chatify.Web.Common;
using Chatify.Web.Common.Attributes;
using Chatify.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GetAllNotificationsResult =
    OneOf.OneOf<Chatify.Application.Common.Models.BaseError,
        Chatify.Shared.Abstractions.Queries.CursorPaged<Chatify.Domain.Entities.UserNotification>>;
using GetUnreadNotificationsResult =
    OneOf.OneOf<Chatify.Application.Common.Models.BaseError,
        System.Collections.Generic.List<Chatify.Domain.Entities.UserNotification>>;
using MarkAllAsReadResult = OneOf.OneOf<Chatify.Application.Common.Models.BaseError, LanguageExt.Unit>;

namespace Chatify.Web.Features.Notifications;

[Authorize]
public class NotificationsController : ApiController
{
    [HttpGet]
    [ProducesBadRequestApiResponse]
    [ProducesOkApiResponse<CursorPaged<UserNotification>>]
    public async Task<IActionResult> Paginated(
        [FromQuery] int pageSize,
        [FromQuery] string? pagingCursor,
        CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<GetAllNotifications, GetAllNotificationsResult>(
            new GetAllNotifications(pageSize, pagingCursor),
            cancellationToken);

        return result.Match(
            err => err.ToBadRequest(),
            Ok);
    }

    [HttpGet]
    [Route("unread")]
    [ProducesBadRequestApiResponse]
    [ProducesOkApiResponse<CursorPaged<UserNotification>>]
    public async Task<IActionResult> Unread(
        CancellationToken cancellationToken = default)
    {
        var result = await QueryAsync<GetUnreadNotifications, GetUnreadNotificationsResult>(
            new GetUnreadNotifications(),
            cancellationToken);
        return result
            .Match(
                err => err.ToBadRequest(),
                Ok);
    }

    [HttpPut]
    [Route("read")]
    [ProducesBadRequestApiResponse]
    [ProducesAcceptedApiResponse]
    public async Task<IActionResult> MarkAsRead(
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<MarkAllAsRead, MarkAllAsReadResult>(
            new MarkAllAsRead(),
            cancellationToken);
        
        return result.Match(err => err.ToBadRequest(), Accepted);
    }
}
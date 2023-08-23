using System.Net;
using Chatify.Application.Notifications.Queries;
using Chatify.Domain.Entities;
using Chatify.Shared.Abstractions.Queries;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GetAllNotificationsResult =
    OneOf.OneOf<LanguageExt.Common.Error,
        Chatify.Shared.Abstractions.Queries.CursorPaged<Chatify.Domain.Entities.UserNotification>>;
using GetUnreadNotificationsResult =
    OneOf.OneOf<LanguageExt.Common.Error, System.Collections.Generic.List<Chatify.Domain.Entities.UserNotification>>;

namespace Chatify.Web.Features.Notifications;

[Authorize]
public class NotificationsController : ApiController
{
    [HttpGet]
    [ProducesResponseType(( int )HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(CursorPaged<UserNotification>), ( int )HttpStatusCode.OK)]
    public Task<IActionResult> Paginated(
        [FromQuery] int pageSize,
        [FromQuery] string pagingCursor,
        CancellationToken cancellationToken = default)
        => QueryAsync<GetAllNotifications, GetAllNotificationsResult>(
                new GetAllNotifications(pageSize, pagingCursor),
                cancellationToken)
            .MatchAsync(
                err => BadRequest(),
                notifications => ( IActionResult )
                    Ok(notifications));

    [HttpGet]
    [Route("unread")]
    [ProducesResponseType(( int )HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(List<UserNotification>), ( int )HttpStatusCode.OK)]
    public Task<IActionResult> Unread(
        [FromBody] GetUnreadNotifications request,
        CancellationToken cancellationToken = default)
        => QueryAsync<GetUnreadNotifications, GetUnreadNotificationsResult>(
                request,
                cancellationToken)
            .MatchAsync(
                err => BadRequest(),
                notifications => ( IActionResult )
                    Ok(notifications));
}
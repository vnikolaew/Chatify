using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Models;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.Notifications.Queries;

using GetAllNotificationsResult = OneOf<BaseError, CursorPaged<UserNotification>>;

public record GetAllNotifications(
    [Required] int PageSize,
    [Required] string? PagingCursor
) : IQuery<GetAllNotificationsResult>;

internal sealed class GetAllNotificationsHandler(
    IIdentityContext identityContext,
    INotificationRepository notifications)
    : IQueryHandler<GetAllNotifications, GetAllNotificationsResult>
{
    public async Task<GetAllNotificationsResult> HandleAsync(GetAllNotifications query,
        CancellationToken cancellationToken = default)
        => await notifications.GetPaginatedForUserAsync(
            identityContext.Id, query.PageSize, query.PagingCursor,
            cancellationToken);
}
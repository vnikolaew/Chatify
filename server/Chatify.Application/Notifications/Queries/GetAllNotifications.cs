using System.ComponentModel.DataAnnotations;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.Notifications.Queries;

using GetAllNotificationsResult = OneOf<Error, CursorPaged<UserNotification>>;

public record GetAllNotifications(
    [Required] int PageSize,
    [Required] string PagingCursor
) : IQuery<GetAllNotificationsResult>;

internal sealed class GetAllNotificationsHandler
    : IQueryHandler<GetAllNotifications, GetAllNotificationsResult>
{
    private readonly IIdentityContext _identityContext;
    private readonly INotificationRepository _notifications;

    public GetAllNotificationsHandler(
        IIdentityContext identityContext,
        INotificationRepository notifications)
    {
        _identityContext = identityContext;
        _notifications = notifications;
    }

    public async Task<GetAllNotificationsResult> HandleAsync(
        GetAllNotifications query,
        CancellationToken cancellationToken = default)
        => await _notifications.GetPaginatedForUserAsync(
            _identityContext.Id, query.PageSize, query.PagingCursor,
            cancellationToken);
}
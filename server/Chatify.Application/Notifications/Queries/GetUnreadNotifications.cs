using Chatify.Application.Common.Models;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using OneOf;

namespace Chatify.Application.Notifications.Queries;

using GetUnreadNotificationsResult = OneOf<BaseError, List<UserNotification>>;

public record GetUnreadNotifications : IQuery<GetUnreadNotificationsResult>;

internal sealed class GetUnreadNotificationsHandler
    : IQueryHandler<GetUnreadNotifications, GetUnreadNotificationsResult>
{
    private readonly IIdentityContext _identityContext;
    private readonly INotificationRepository _notifications;

    public GetUnreadNotificationsHandler(
        IIdentityContext identityContext,
        INotificationRepository notifications)
    {
        _identityContext = identityContext;
        _notifications = notifications;
    }

    public async Task<GetUnreadNotificationsResult> HandleAsync(
        GetUnreadNotifications _,
        CancellationToken cancellationToken = default)
    {
        var allNotifications = await _notifications.AllForUserAsync(
            _identityContext.Id,
            cancellationToken);
        
        return allNotifications
            .Where(_ => !_.Read)
            .ToList();
    }
}
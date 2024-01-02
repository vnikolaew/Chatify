using Chatify.Application.Common.Models;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using OneOf;

namespace Chatify.Application.Notifications.Queries;

using GetUnreadNotificationsResult = OneOf<BaseError, List<UserNotification>>;

public record GetUnreadNotifications : IQuery<GetUnreadNotificationsResult>;

internal sealed class GetUnreadNotificationsHandler(
    IIdentityContext identityContext,
    INotificationRepository notifications)
    : IQueryHandler<GetUnreadNotifications, GetUnreadNotificationsResult>
{
    public async Task<GetUnreadNotificationsResult> HandleAsync(GetUnreadNotifications _,
        CancellationToken cancellationToken = default)
    {
        var allNotifications = await notifications.AllForUserAsync(
            identityContext.Id,
            cancellationToken);

        return allNotifications
            .Where(n => !n.Read)
            .ToList();
    }
}
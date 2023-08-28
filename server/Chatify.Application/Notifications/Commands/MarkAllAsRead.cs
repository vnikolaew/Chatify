using Chatify.Application.Common.Models;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using LanguageExt;
using OneOf;

namespace Chatify.Application.Notifications.Commands;

using MarkAllAsReadResult = OneOf<BaseError, Unit>;

public record MarkAllAsRead : ICommand<MarkAllAsReadResult>;

internal sealed class MarkAllAsReadHandler
    : ICommandHandler<MarkAllAsRead, MarkAllAsReadResult>
{
    private readonly IIdentityContext _identityContext;
    private readonly INotificationRepository _notifications;

    public MarkAllAsReadHandler(
        IIdentityContext identityContext,
        INotificationRepository notifications)
    {
        _identityContext = identityContext;
        _notifications = notifications;
    }

    public async Task<MarkAllAsReadResult> HandleAsync(
        MarkAllAsRead command,
        CancellationToken cancellationToken = default)
    {
        var notifications = await _notifications
            .AllForUserAsync(_identityContext.Id, cancellationToken);
        foreach ( var userNotification in notifications )
        {
            await _notifications.UpdateAsync(
                userNotification,
                notification => notification.Read = true,
                cancellationToken);
        }

        return Unit.Default;
    }
}
using Chatify.Application.Common.Models;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using LanguageExt;
using OneOf;

namespace Chatify.Application.Notifications.Commands;

using MarkAllAsReadResult = OneOf<BaseError, Unit>;

public record MarkAllAsRead : ICommand<MarkAllAsReadResult>;

internal sealed class MarkAllAsReadHandler(IIdentityContext identityContext,
        INotificationRepository notifications)
    : ICommandHandler<MarkAllAsRead, MarkAllAsReadResult>
{
    public async Task<MarkAllAsReadResult> HandleAsync(
        MarkAllAsRead command,
        CancellationToken cancellationToken = default)
    {
        var userNotifications = await notifications
            .AllForUserAsync(identityContext.Id, cancellationToken);
        foreach ( var userNotification in userNotifications )
        {
            await notifications.UpdateAsync(
                userNotification,
                notification => notification.Read = true,
                cancellationToken);
        }

        return Unit.Default;
    }
}
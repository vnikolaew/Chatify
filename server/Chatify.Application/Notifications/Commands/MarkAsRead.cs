using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Models;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using LanguageExt;
using OneOf;

namespace Chatify.Application.Notifications.Commands;

using MarkAsReadResult = OneOf<BaseError, Unit>;

public record MarkAsRead(
    [Required] Guid NotificationId
) : ICommand<MarkAsReadResult>;

internal sealed class MarkAsReadHandler
    : ICommandHandler<MarkAsRead, MarkAsReadResult>
{
    private readonly IIdentityContext _identityContext;
    private readonly INotificationRepository _notifications;

    public MarkAsReadHandler(
        IIdentityContext identityContext,
        INotificationRepository notifications)
    {
        _identityContext = identityContext;
        _notifications = notifications;
    }

    public async Task<MarkAsReadResult> HandleAsync(
        MarkAsRead command,
        CancellationToken cancellationToken = default)
    {
        var notification = await _notifications
            .GetAsync(command.NotificationId, cancellationToken);
        if ( notification is null ) return new MarkAsReadResult()!;

        await _notifications
            .UpdateAsync(notification,
                notification => notification.Read = true,
                cancellationToken);

        return Unit.Default;
    }
}
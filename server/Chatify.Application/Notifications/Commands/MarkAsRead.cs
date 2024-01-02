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

internal sealed class MarkAsReadHandler(INotificationRepository notifications)
    : ICommandHandler<MarkAsRead, MarkAsReadResult>
{
    public async Task<MarkAsReadResult> HandleAsync(
        MarkAsRead command,
        CancellationToken cancellationToken = default)
    {
        var notification = await notifications
            .GetAsync(command.NotificationId, cancellationToken);
        if ( notification is null ) return new MarkAsReadResult()!;

        await notifications
            .UpdateAsync(notification,
                notification => notification.Read = true,
                cancellationToken);

        return Unit.Default;
    }
}
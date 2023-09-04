using System.ComponentModel.DataAnnotations;
using Chatify.Application.User.Common;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Users;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;

namespace Chatify.Application.User.Commands;

using ChangeUserStatusResult = OneOf.OneOf<UserNotFound, Unit>;

public record ChangeUserStatus(
    [Required] UserStatus NewStatus
) : ICommand<ChangeUserStatusResult>;

internal sealed class ChangeUserStatusHandler(IIdentityContext identityContext,
        IDomainRepository<Domain.Entities.User, Guid> users,
        IClock clock,
        IEventDispatcher eventDispatcher)
    : ICommandHandler<ChangeUserStatus, ChangeUserStatusResult>
{
    public async Task<ChangeUserStatusResult> HandleAsync(
        ChangeUserStatus command,
        CancellationToken cancellationToken = default)
    {
        var user = await users.UpdateAsync(identityContext.Id, user =>
        {
            user.Status = command.NewStatus;
            user.UpdatedAt = clock.Now;
        }, cancellationToken);
        if ( user is null ) return new UserNotFound();

        await eventDispatcher.PublishAsync(new UserChangedStatusEvent
        {
            UserId = identityContext.Id,
            NewStatus = command.NewStatus,
            Timestamp = clock.Now
        }, cancellationToken);
        
        return Unit.Default;
    }
}
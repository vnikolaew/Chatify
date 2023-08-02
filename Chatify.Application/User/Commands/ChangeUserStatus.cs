using System.ComponentModel.DataAnnotations;
using Chatify.Domain.Common;
using Chatify.Domain.Events.Users;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using LanguageExt.Common;

namespace Chatify.Application.User.Commands;

using ChangeUserStatusResult = Either<Error, Unit>;

public record ChangeUserStatus(
    [Required] sbyte NewStatus
) : ICommand<ChangeUserStatusResult>;

internal sealed class ChangeUserStatusHandler
    : ICommandHandler<ChangeUserStatus, ChangeUserStatusResult>
{
    private readonly IIdentityContext _identityContext;
    private readonly IClock _clock;
    private readonly IDomainRepository<Domain.Entities.User, Guid> _users;
    private readonly IEventDispatcher _eventDispatcher;
    

    public ChangeUserStatusHandler(
        IIdentityContext identityContext,
        IDomainRepository<Domain.Entities.User, Guid> users,
        IClock clock,
        IEventDispatcher eventDispatcher)
    {
        _identityContext = identityContext;
        _users = users;
        _clock = clock;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<ChangeUserStatusResult> HandleAsync(
        ChangeUserStatus command,
        CancellationToken cancellationToken = default)
    {
        var user = await _users.UpdateAsync(_identityContext.Id, user =>
        {
            user.Status = command.NewStatus;
            user.UpdatedAt = _clock.Now;
        }, cancellationToken);
        if (user is null) return Error.New("Status update was unsuccessful");

        await _eventDispatcher.PublishAsync(new UserChangedStatusEvent
        {
            UserId = _identityContext.Id,
            NewStatus = command.NewStatus,
            Timestamp = _clock.Now
        }, cancellationToken);
        
        return Unit.Default;
    }
}
using System.ComponentModel.DataAnnotations;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Friendships;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using OneOf;

namespace Chatify.Application.Friendships.Commands;

using DeclineFriendInvitationResult = OneOf<FriendInviteNotFoundError, FriendInviteInvalidStateError, Unit>;

public record DeclineFriendInvitation([Required] Guid InviteId) : ICommand<DeclineFriendInvitationResult>;

internal sealed class DeclineFriendInvitationHandler :
    ICommandHandler<DeclineFriendInvitation, DeclineFriendInvitationResult>
{
    private readonly IIdentityContext _identityContext;
    private readonly IDomainRepository<FriendInvitation, Guid> _friendInvites;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IClock _clock;

    public DeclineFriendInvitationHandler(
        IIdentityContext identityContext,
        IDomainRepository<FriendInvitation, Guid> friendInvites,
        IEventDispatcher eventDispatcher,
        IClock clock)
    {
        _identityContext = identityContext;
        _friendInvites = friendInvites;
        _eventDispatcher = eventDispatcher;
        _clock = clock;
    }

    public async Task<DeclineFriendInvitationResult> HandleAsync(
        DeclineFriendInvitation command,
        CancellationToken cancellationToken = default)
    {
        // Check if friend invite exists and is in a 'Pending' state:
        var friendInvite = await _friendInvites.GetAsync(command.InviteId, cancellationToken: cancellationToken);
        if ( friendInvite is null ) return new FriendInviteNotFoundError(command.InviteId);
        
        if (friendInvite.Status != FriendInvitationStatus.Pending)
        {
            return new FriendInviteInvalidStateError(friendInvite.Status);
        }
        if (friendInvite.InviteeId != _identityContext.Id)
        {
            return new FriendInviteInvalidStateError(friendInvite.Status);
        }

        // Update friend invite:
        await _friendInvites.UpdateAsync(
            friendInvite.Id,
            invite =>
            {
                invite.Status = FriendInvitationStatus.Declined;
                invite.UpdatedAt = _clock.Now;
            },
            cancellationToken);

        await _eventDispatcher.PublishAsync(new FriendInvitationDeclinedEvent
        {
            InviterId = friendInvite.InviterId,
            InviteeId = friendInvite.InviteeId,
            Timestamp = _clock.Now,
            InviteId = friendInvite.Id
        }, cancellationToken);
        
        return Unit.Default;
    }
}
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

internal sealed class DeclineFriendInvitationHandler(IIdentityContext identityContext,
        IDomainRepository<FriendInvitation, Guid> friendInvites,
        IEventDispatcher eventDispatcher,
        IClock clock)
    :
        ICommandHandler<DeclineFriendInvitation, DeclineFriendInvitationResult>
{
    public async Task<DeclineFriendInvitationResult> HandleAsync(
        DeclineFriendInvitation command,
        CancellationToken cancellationToken = default)
    {
        // Check if friend invite exists and is in a 'Pending' state:
        var friendInvite = await friendInvites.GetAsync(command.InviteId, cancellationToken: cancellationToken);
        if ( friendInvite is null ) return new FriendInviteNotFoundError(command.InviteId);
        
        if (friendInvite.Status != FriendInvitationStatus.Pending)
        {
            return new FriendInviteInvalidStateError(friendInvite.Status);
        }
        if (friendInvite.InviteeId != identityContext.Id)
        {
            return new FriendInviteInvalidStateError(friendInvite.Status);
        }

        // Update friend invite:
        await friendInvites.UpdateAsync(
            friendInvite,
            invite =>
            {
                invite.Status = FriendInvitationStatus.Declined;
                invite.UpdatedAt = clock.Now;
            },
            cancellationToken);

        await eventDispatcher.PublishAsync(new FriendInvitationDeclinedEvent
        {
            InviterId = friendInvite.InviterId,
            InviteeId = friendInvite.InviteeId,
            Timestamp = clock.Now,
            InviteId = friendInvite.Id
        }, cancellationToken);
        
        return Unit.Default;
    }
}
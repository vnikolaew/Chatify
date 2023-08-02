﻿using System.ComponentModel.DataAnnotations;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Friendships;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using LanguageExt.Common;

namespace Chatify.Application.Friendships.Commands;

using DeclineFriendInvitationResult = Either<Error, Unit>;

public record DeclineFriendInvitation([Required] Guid InviteId) : ICommand<DeclineFriendInvitationResult>;

internal sealed class DeclineFriendInvitationHandler :
    ICommandHandler<DeclineFriendInvitation, DeclineFriendInvitationResult>
{
    private readonly IIdentityContext _identityContext;
    private readonly IDomainRepository<FriendInvitation, Guid> _friendInvites;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IDomainRepository<FriendsRelation, Guid> _friends;
    private readonly IClock _clock;

    public DeclineFriendInvitationHandler(
        IIdentityContext identityContext,
        IDomainRepository<FriendInvitation, Guid> friendInvites,
        IEventDispatcher eventDispatcher,
        IDomainRepository<FriendsRelation, Guid> friends,
        IClock clock)
    {
        _identityContext = identityContext;
        _friendInvites = friendInvites;
        _eventDispatcher = eventDispatcher;
        _friends = friends;
        _clock = clock;
    }

    public async Task<DeclineFriendInvitationResult> HandleAsync(
        DeclineFriendInvitation command,
        CancellationToken cancellationToken = default)
    {
        // Check if friend invite exists and is in a 'Pending' state:
        var friendInvite = await _friendInvites.GetAsync(command.InviteId, cancellationToken: cancellationToken);
        if (friendInvite is null || friendInvite.Status != (sbyte)FriendInvitationStatus.Pending)
        {
            return Error.New("Friend invitation does not exist or is not in a pending state.");
        }
        if (friendInvite.InviteeId != _identityContext.Id)
        {
            return Error.New("Current user is not related to this friend invitation.");
        }

        // Update friend invite:
        await _friendInvites.UpdateAsync(
            friendInvite.Id,
            invite =>
            {
                invite.Status = (sbyte)FriendInvitationStatus.Declined;
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
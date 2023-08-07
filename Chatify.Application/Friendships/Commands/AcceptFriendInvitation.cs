using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Contracts;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Friendships;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using OneOf;

namespace Chatify.Application.Friendships.Commands;

using AcceptFriendInvitationResult = OneOf<FriendInviteNotFoundError, FriendInviteInvalidStateError, Guid>;

public record FriendInviteNotFoundError(Guid InviteId = default, Guid InviteeId = default);

public record FriendInviteInvalidStateError(FriendInvitationStatus Status);

public record AcceptFriendInvitation([Required] Guid InviteId) : ICommand<AcceptFriendInvitationResult>;

internal sealed class AcceptFriendInvitationHandler :
    ICommandHandler<AcceptFriendInvitation, AcceptFriendInvitationResult>
{
    private readonly IIdentityContext _identityContext;
    private readonly IDomainRepository<FriendInvitation, Guid> _friendInvites;
    private readonly IClock _clock;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IDomainRepository<FriendsRelation, Guid> _friends;

    public AcceptFriendInvitationHandler(
        IIdentityContext identityContext,
        IDomainRepository<FriendInvitation, Guid> friendInvites,
        IDomainRepository<FriendsRelation, Guid> friends,
        IClock clock, IEventDispatcher eventDispatcher,
        IGuidGenerator guidGenerator)
    {
        _identityContext = identityContext;
        _friendInvites = friendInvites;
        _friends = friends;
        _clock = clock;
        _eventDispatcher = eventDispatcher;
        _guidGenerator = guidGenerator;
    }

    public async Task<AcceptFriendInvitationResult> HandleAsync(
        AcceptFriendInvitation command,
        CancellationToken cancellationToken = default)
    {
        // Check if friend invite exists and is in a 'Pending' state:
        var friendInvite = await _friendInvites.GetAsync(command.InviteId, cancellationToken: cancellationToken);
        if ( friendInvite is null ) return new FriendInviteNotFoundError(command.InviteId);
        if (friendInvite.Status != (sbyte)FriendInvitationStatus.Pending)
        {
            return new FriendInviteInvalidStateError((FriendInvitationStatus)
                friendInvite.Status);
        }

        var friendsRelationId = _guidGenerator.New();
        var friendsRelation = new FriendsRelation
        {
            Id = friendsRelationId,
            FriendOneId = _identityContext.Id,
            FriendTwoId = friendInvite.InviteeId,
            CreatedAt = _clock.Now
        };
        
        // Save new friendship:
        await _friends.SaveAsync(friendsRelation, cancellationToken);
        
        // Update friend invite:
        await _friendInvites.UpdateAsync(
            friendInvite.Id,
            invite =>
            {
                invite.Status = (sbyte)FriendInvitationStatus.Accepted;
                invite.UpdatedAt = _clock.Now;
            },
            cancellationToken);
        
        await _eventDispatcher.PublishAsync(new FriendInvitationAcceptedEvent
        {
            FriendsRelationId = friendsRelationId,
            InviterId = friendInvite.InviterId,
            InviteeId = friendInvite.InviteeId,
            InviteId = friendInvite.Id,
            Timestamp = _clock.Now
        }, cancellationToken);
        
        return friendsRelation.Id;
    }
}
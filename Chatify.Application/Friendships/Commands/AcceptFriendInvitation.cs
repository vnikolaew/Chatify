using System.ComponentModel.DataAnnotations;
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

using AcceptFriendInvitationResult = Either<Error, Guid>;

public record AcceptFriendInvitation([Required] Guid InviteId) : ICommand<AcceptFriendInvitationResult>;

internal sealed class AcceptFriendInvitationHandler :
    ICommandHandler<AcceptFriendInvitation, AcceptFriendInvitationResult>
{
    private readonly IIdentityContext _identityContext;
    private readonly IDomainRepository<FriendInvitation, Guid> _friendInvites;
    private readonly IClock _clock;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IDomainRepository<FriendsRelation, Guid> _friends;

    public AcceptFriendInvitationHandler(
        IIdentityContext identityContext,
        IDomainRepository<FriendInvitation, Guid> friendInvites,
        IDomainRepository<FriendsRelation, Guid> friends,
        IClock clock, IEventDispatcher eventDispatcher)
    {
        _identityContext = identityContext;
        _friendInvites = friendInvites;
        _friends = friends;
        _clock = clock;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<AcceptFriendInvitationResult> HandleAsync(
        AcceptFriendInvitation command,
        CancellationToken cancellationToken = default)
    {
        // Check if friend invite exists and is in a 'Pending' state:
        var friendInvite = await _friendInvites.GetAsync(command.InviteId, cancellationToken: cancellationToken);
        if (friendInvite is null || friendInvite.Status != (sbyte)FriendInvitationStatus.Pending)
        {
            return Error.New("Friend invitation does not exist or is not in a pending state.");
        }

        var friendsRelation = new FriendsRelation
        {
            Id = Guid.NewGuid(),
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
            InviterId = friendInvite.InviterId,
            InviteeId = friendInvite.InviteeId,
            InviteId = friendInvite.Id,
            Timestamp = _clock.Now
        }, cancellationToken);
        return friendsRelation.Id;
    }
}
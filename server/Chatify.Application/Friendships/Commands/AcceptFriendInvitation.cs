using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common;
using Chatify.Application.Common.Models;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Friendships;
using Chatify.Domain.Events.Groups;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Common;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using Chatify.Shared.Infrastructure.Common.Extensions;
using OneOf;

namespace Chatify.Application.Friendships.Commands;

using AcceptFriendInvitationResult = OneOf<FriendInviteNotFoundError, FriendInviteInvalidStateError, Guid>;

public record FriendInviteNotFoundError(Guid InviteId = default, Guid InviteeId = default)
    : BaseError("Friend invitation not found.");

public record FriendInviteInvalidStateError(FriendInvitationStatus Status)
    : BaseError("Friend invitation is in an invalid state.");

public record AcceptFriendInvitation([Required] Guid InviteId) : ICommand<AcceptFriendInvitationResult>;

internal sealed class AcceptFriendInvitationHandler(
    IIdentityContext identityContext,
    IDomainRepository<FriendInvitation, Guid> friendInvites,
    IFriendshipsRepository friends,
    IClock clock,
    IEventDispatcher eventDispatcher,
    IGuidGenerator guidGenerator,
    IChatGroupRepository groups,
    IChatGroupMemberRepository members,
    IUserRepository users)
    :
        BaseCommandHandler<AcceptFriendInvitation, AcceptFriendInvitationResult>(eventDispatcher, identityContext,
            clock)
{
    public override async Task<AcceptFriendInvitationResult> HandleAsync(
        AcceptFriendInvitation command,
        CancellationToken cancellationToken = default)
    {
        // Check if friend invite exists and is in a 'Pending' state:
        var friendInvite = await friendInvites.GetAsync(command.InviteId, cancellationToken: cancellationToken);
        if ( friendInvite is null ) return new FriendInviteNotFoundError(command.InviteId);
        if ( friendInvite.Status != ( sbyte )FriendInvitationStatus.Pending )
        {
            return new FriendInviteInvalidStateError(
                friendInvite.Status);
        }

        var friendsRelationId = guidGenerator.New();
        var groupId = guidGenerator.New();
        var friendsRelation = new FriendsRelation
        {
            Id = friendsRelationId,
            FriendOneId = identityContext.Id,
            FriendTwoId = friendInvite.InviterId,
            GroupId = groupId,
            CreatedAt = clock.Now
        };

        // Save new friendship:
        await friends.SaveAsync(friendsRelation, cancellationToken);

        // Create new DM group between the two users and add them as group members: 
        var group = new ChatGroup
        {
            Id = groupId,
            CreatedAt = clock.Now,
            AdminIds = new HashSet<Guid> { identityContext.Id, friendInvite.InviteeId },
            CreatorId = friendInvite.InviterId,
        };
        await groups.SaveAsync(group, cancellationToken);

        var inviter = await users.GetAsync(friendInvite.InviterId, cancellationToken);
        var groupMembers = new ChatGroupMember[]
        {
            new()
            {
                Id = guidGenerator.New(),
                CreatedAt = clock.Now,
                ChatGroupId = group.Id,
                UserId = identityContext.Id,
                Username = identityContext.Username,
                MembershipType = 0
            },
            new()
            {
                Id = guidGenerator.New(),
                CreatedAt = clock.Now,
                ChatGroupId = group.Id,
                UserId = friendInvite.InviterId,
                Username = inviter!.Username,
                MembershipType = 0,
            }
        };

        await groupMembers.Select(m => members.SaveAsync(m, cancellationToken));

        // Update friend invite:
        await friendInvites.UpdateAsync(
            friendInvite,
            invite =>
            {
                invite.Status = FriendInvitationStatus.Accepted;
                invite.UpdatedAt = clock.Now;
            },
            cancellationToken);

        var events = new IDomainEvent[]
        {
            new ChatGroupCreatedEvent
            {
                CreatorId = group.CreatorId,
                Timestamp = group.CreatedAt.DateTime,
                Name = group.Name,
                GroupId = group.Id
            },
            new FriendInvitationAcceptedEvent
            {
                FriendsRelationId = friendsRelationId,
                InviterId = friendInvite.InviterId,
                InviteeId = friendInvite.InviteeId,
                InviteId = friendInvite.Id,
                NewGroupId = group.Id,
                Timestamp = clock.Now
            }
        };
        
        await eventDispatcher.PublishAsync(events, cancellationToken);
        return friendsRelation.Id;
    }
}
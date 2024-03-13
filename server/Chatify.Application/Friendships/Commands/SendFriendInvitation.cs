using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Contracts;
using Chatify.Application.User.Common;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Friendships;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Common;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using OneOf;

namespace Chatify.Application.Friendships.Commands;

using SendFriendInvitationResult = OneOf<UserNotFound, FriendInviteNotFoundError, Guid>;

public record SendFriendInvitation([Required] Guid InviteeId) : ICommand<SendFriendInvitationResult>;

internal sealed class SendFriendInvitationHandler(IFriendInvitationRepository friendInvites,
        IIdentityContext identityContext,
        IUserRepository users,
        IClock clock,
        IEventDispatcher eventDispatcher,
        IGuidGenerator guidGenerator)
    : ICommandHandler<SendFriendInvitation, SendFriendInvitationResult>
{
    public async Task<SendFriendInvitationResult> HandleAsync(
        SendFriendInvitation command,
        CancellationToken cancellationToken = default)
    {
        var invitee = await users.GetAsync(command.InviteeId, cancellationToken);
        if ( invitee is null ) return new UserNotFound();

        var existingInvites = await friendInvites
            .AllSentByUserAsync(identityContext.Id, cancellationToken);
        if (existingInvites.Any(i => i.InviteeId == command.InviteeId))
        {
            return new FriendInviteNotFoundError(InviteeId: command.InviteeId);
        }

        var friendInviteId = guidGenerator.New();
        var friendInvite = new FriendInvitation
        {
            InviteeId = invitee.Id,
            Id = friendInviteId,
            Status = (sbyte)FriendInvitationStatus.Pending,
            CreatedAt = clock.Now,
            InviterId = identityContext.Id
        };

        var invite = await friendInvites.SaveAsync(friendInvite, cancellationToken);
        await eventDispatcher.PublishAsync(new FriendInvitationSentEvent
        {
            Id = friendInviteId,
            InviterId = friendInvite.InviterId,
            InviteeId = friendInvite.InviteeId,
            Timestamp = clock.Now,
            InviterUsername = identityContext.Username
        }, cancellationToken);
        return invite.Id;
    }
}
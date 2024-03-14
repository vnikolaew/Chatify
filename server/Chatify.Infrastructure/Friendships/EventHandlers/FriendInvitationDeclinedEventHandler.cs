using Chatify.Domain.Entities;
using Chatify.Domain.Events.Friendships;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Common;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.Friendships.EventHandlers;

internal sealed class FriendInvitationDeclinedEventHandler
    (
        IHubContext<ChatifyHub, IChatifyHubClient> hubContext,
        IUserRepository users,
        IGuidGenerator guidGenerator,
        IClock clock,
        INotificationRepository notifications)
    : IEventHandler<FriendInvitationDeclinedEvent>
{
    public async Task HandleAsync(
        FriendInvitationDeclinedEvent @event,
        CancellationToken cancellationToken = default)
    {
        var invitee = await users.GetAsync(@event.InviteeId, cancellationToken);
        if(invitee is null) return;
        
        await CreateAndSaveNotification(@event, invitee, cancellationToken);

        await hubContext
            .Clients
            .User(@event.InviterId.ToString())
            .FriendInvitationDeclined(new FriendInvitationDeclined(
                invitee.Id,
                invitee.Username,
                @event.Timestamp,
                new Dictionary<string, string>
                {
                    { nameof(UserNotification.Metadata.UserMedia), invitee.ProfilePicture.MediaUrl }
                }));
    }

    private async Task CreateAndSaveNotification(
        FriendInvitationDeclinedEvent @event,
        Domain.Entities.User invitee,
        CancellationToken cancellationToken)
    {
        var summary = $"{invitee.Username} declined your friend request.";
        
        var notification = new DeclinedFriendInvitationNotification
        {
            Id = guidGenerator.New(),
            UserId = @event.InviteeId,
            Type = UserNotificationType.DeclinedFriendInvite,
            Summary = summary,
            CreatedAt = clock.Now,
            Metadata = new UserNotificationMetadata
            {
                UserMedia = invitee.ProfilePicture
            },
            InviteId = @event.InviteId,
            InviterId = @event.InviterId
        };
        await notifications.SaveAsync(notification, cancellationToken);
    }
}
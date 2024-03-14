using Chatify.Domain.Common;
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

internal sealed class FriendInvitationSentEventHandler
(IHubContext<ChatifyHub, IChatifyHubClient> hubContext,
    IUserRepository users,
    IDomainRepository<UserNotification, Guid> notifications,
    IGuidGenerator guidGenerator,
    IClock clock
) : IEventHandler<FriendInvitationSentEvent>
{
    public async Task HandleAsync(
        FriendInvitationSentEvent @event,
        CancellationToken cancellationToken = default)
    {
        var inviter = await users.GetAsync(@event.InviterId, cancellationToken);

        // Save a new notification for Invitee:
        var notification = new IncomingFriendInvitationNotification
        {
            Id = guidGenerator.New(),
            CreatedAt = clock.Now,
            UserId = @event.InviteeId,
            Type = UserNotificationType.IncomingFriendInvite,
            Summary = $"{@event.InviterUsername} sent you a friend invitation.",
            Metadata = new UserNotificationMetadata
            {
                UserMedia = inviter.ProfilePicture
            },
            InviteId = @event.Id
        };
        await notifications.SaveAsync(notification, cancellationToken);

        await hubContext
            .Clients
            .User(@event.InviteeId.ToString())
            .ReceiveFriendInvitation(new ReceiveFriendInvitation(
                @event.InviterId,
                @event.InviterUsername,
                @event.Timestamp,
                new Dictionary<string, string>
                {
                    { nameof(UserNotification.Metadata.UserMedia),
                        notification.Metadata.UserMedia.MediaUrl }
                }));
    }
}
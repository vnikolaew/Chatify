using Chatify.Application.Common.Contracts;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Friendships;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
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
        await CreateAndSaveNotification(@event, invitee!, cancellationToken);

        await hubContext
            .Clients
            .User(@event.InviterId.ToString())
            .FriendInvitationDeclined(new FriendInvitationDeclined(
                invitee!.Id,
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
        var notification = new DeclinedFriendInvitationNotification
        {
            Id = guidGenerator.New(),
            User = invitee,
            UserId = @event.InviteeId,
            Type = UserNotificationType.DeclinedFriendInvite,
            Summary = $"{invitee!.Username} declined your friend request.",
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
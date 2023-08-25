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
        await CreateAndSaveNotification(@event, cancellationToken);
        await hubContext
            .Clients
            .User(@event.InviterId.ToString())
            .FriendInvitationDeclined(new FriendInvitationDeclined(
                @event.InviteeId,
                @event.Timestamp));
    }
    
    private async Task CreateAndSaveNotification(
        FriendInvitationDeclinedEvent @event,
        CancellationToken cancellationToken)
    {
        var inviter = await users.GetAsync(@event.InviterId, cancellationToken);
        var notification = new DeclinedFriendInvitationNotification
        {
            Id = guidGenerator.New(),
            User = inviter,
            UserId = @event.InviteeId,
            Type = UserNotificationType.DeclinedFriendInvite,
            Summary = $"{inviter.Username} declined your friend request.",
            CreatedAt = clock.Now,
            Metadata = new UserNotificationMetadata
            {
                UserMedia = inviter.ProfilePicture
            },
            InviteId = @event.InviteId,
            InviterId = @event.InviterId
        };
        await notifications.SaveAsync(notification, cancellationToken);
    }
}
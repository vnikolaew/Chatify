using Chatify.Application.Common.Contracts;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Friendships;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Caching.Extensions;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace Chatify.Infrastructure.Friendships.EventHandlers;

internal sealed class FriendInvitationAcceptedEventHandler(
        IHubContext<ChatifyHub, IChatifyHubClient> hubContext,
        INotificationRepository notifications,
        IUserRepository users,
        IDatabase cache,
        IGuidGenerator guidGenerator,
        IClock clock)
    : IEventHandler<FriendInvitationAcceptedEvent>
{
    public async Task HandleAsync(
        FriendInvitationAcceptedEvent @event,
        CancellationToken cancellationToken = default)
    {
        var cacheSaveTasks = new Task[]
        {
            cache.AddUserFriendAsync(
                @event.InviterId,
                @event.InviteeId,@event.Timestamp),
            cache.AddUserFriendAsync(
                @event.InviteeId,
                @event.InviterId,
                @event.Timestamp),
        };
        await Task.WhenAll(cacheSaveTasks);

        await CreateAndSaveNotification(@event, cancellationToken);
        await hubContext
            .Clients
            .User(@event.InviterId.ToString())
            .FriendInvitationAccepted(new FriendInvitationAccepted(
                @event.InviteeId,
                @event.Timestamp));
    }

    private async Task CreateAndSaveNotification(
        FriendInvitationAcceptedEvent @event,
        CancellationToken cancellationToken)
    {
        var invitee = await users.GetAsync(@event.InviteeId, cancellationToken);
        var notification = new AcceptedFriendInvitationNotification
        {
            Id = guidGenerator.New(),
            UserId = @event.InviterId,
            Type = UserNotificationType.AcceptedFriendInvite,
            Summary = $"{invitee!.Username} accepted your friend request.",
            CreatedAt = clock.Now,
            Metadata = new UserNotificationMetadata
            {
                UserMedia = invitee.ProfilePicture
            },
            InviteId = @event.InviteId,
            InviterId = @event.InviterId,
            ChatGroupId = @event.NewGroupId
        };
        await notifications.SaveAsync(notification, cancellationToken);
    }
}
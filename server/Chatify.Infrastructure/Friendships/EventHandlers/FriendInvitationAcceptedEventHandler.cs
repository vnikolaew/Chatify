using Chatify.Application.Common.Contracts;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Friendships;
using Chatify.Domain.Repositories;
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
    
    private static RedisKey GetUserFriendsCacheKey(Guid userId)
        => $"user:{userId}:friends";
    
    public async Task HandleAsync(
        FriendInvitationAcceptedEvent @event,
        CancellationToken cancellationToken = default)
    {
        var cacheSaveTasks = new Task[]
        {
            cache.SortedSetAddAsync(
                GetUserFriendsCacheKey(@event.InviterId),
                new RedisValue(@event.InviteeId.ToString()),
                @event.Timestamp.Ticks,
                SortedSetWhen.NotExists),
            cache.SortedSetAddAsync(
                GetUserFriendsCacheKey(@event.InviteeId),
                new RedisValue(@event.InviterId.ToString()),
                @event.Timestamp.Ticks,
                SortedSetWhen.NotExists),
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
        var inviter = await users.GetAsync(@event.InviterId, cancellationToken);
        var notification = new AcceptedFriendInvitationNotification
        {
            Id = guidGenerator.New(),
            UserId = @event.InviteeId,
            Type = UserNotificationType.AcceptedFriendInvite,
            Summary = $"{inviter.Username} accepted your friend request.",
            CreatedAt = clock.Now,
            Metadata = new UserNotificationMetadata
            {
                UserMedia = inviter.ProfilePicture
            },
            InviteId = @event.InviteId,
            InviterId = @event.InviterId,
            ChatGroupId = @event.NewGroupId
        };
        await notifications.SaveAsync(notification, cancellationToken);
    }
}
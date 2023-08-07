using Chatify.Domain.Events.Friendships;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace Chatify.Infrastructure.Friendships.EventHandlers;

internal sealed class FriendInvitationAcceptedEventHandler : IEventHandler<FriendInvitationAcceptedEvent>
{
    private readonly IHubContext<ChatifyHub, IChatifyHubClient> _hubContext;
    private readonly IDatabase _cache;

    public FriendInvitationAcceptedEventHandler(
        IHubContext<ChatifyHub, IChatifyHubClient> hubContext,
        IDatabase cache)
    {
        _hubContext = hubContext;
        _cache = cache;
    }

    public async Task HandleAsync(
        FriendInvitationAcceptedEvent @event,
        CancellationToken cancellationToken = default)
    {
        var cacheSaveTasks = new Task[]
        {
            _cache.SortedSetAddAsync(
                new RedisKey($"user:{@event.InviterId}:friends"),
                new RedisValue(@event.InviteeId.ToString()),
                @event.Timestamp.Ticks,
                SortedSetWhen.NotExists),
            _cache.SortedSetAddAsync(
                new RedisKey($"user:{@event.InviteeId}:friends"),
                new RedisValue(@event.InviterId.ToString()),
                @event.Timestamp.Ticks,
                SortedSetWhen.NotExists),
        };

        await Task.WhenAll(cacheSaveTasks);
        await _hubContext
            .Clients
            .User(@event.InviterId.ToString())
            .FriendInvitationAccepted(new FriendInvitationAccepted(
                @event.InviteeId,
                @event.Timestamp));
    }
}
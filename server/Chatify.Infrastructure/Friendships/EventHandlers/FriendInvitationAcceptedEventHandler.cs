using Chatify.Application.Common.Contracts;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Friendships;
using Chatify.Domain.Events.Groups;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace Chatify.Infrastructure.Friendships.EventHandlers;

internal sealed class FriendInvitationAcceptedEventHandler
    : IEventHandler<FriendInvitationAcceptedEvent>
{
    private readonly IHubContext<ChatifyHub, IChatifyHubClient> _hubContext;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IChatGroupRepository _groups;
    private readonly IClock _clock;
    private readonly IDatabase _cache;

    public FriendInvitationAcceptedEventHandler(
        IHubContext<ChatifyHub, IChatifyHubClient> hubContext,
        IDatabase cache,
        IChatGroupRepository groups, IGuidGenerator guidGenerator, IClock clock, IEventDispatcher eventDispatcher)
    {
        _hubContext = hubContext;
        _cache = cache;
        _groups = groups;
        _guidGenerator = guidGenerator;
        _clock = clock;
        _eventDispatcher = eventDispatcher;
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
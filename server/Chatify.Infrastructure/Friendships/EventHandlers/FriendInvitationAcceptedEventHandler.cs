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

internal sealed class FriendInvitationAcceptedEventHandler(IHubContext<ChatifyHub, IChatifyHubClient> hubContext,
        IDatabase cache,
        IChatGroupRepository groups, IGuidGenerator guidGenerator, IClock clock, IEventDispatcher eventDispatcher)
    : IEventHandler<FriendInvitationAcceptedEvent>
{
    private readonly IEventDispatcher _eventDispatcher = eventDispatcher;
    private readonly IGuidGenerator _guidGenerator = guidGenerator;
    private readonly IChatGroupRepository _groups = groups;
    private readonly IClock _clock = clock;

    public async Task HandleAsync(
        FriendInvitationAcceptedEvent @event,
        CancellationToken cancellationToken = default)
    {
        var cacheSaveTasks = new Task[]
        {
            cache.SortedSetAddAsync(
                new RedisKey($"user:{@event.InviterId}:friends"),
                new RedisValue(@event.InviteeId.ToString()),
                @event.Timestamp.Ticks,
                SortedSetWhen.NotExists),
            cache.SortedSetAddAsync(
                new RedisKey($"user:{@event.InviteeId}:friends"),
                new RedisValue(@event.InviterId.ToString()),
                @event.Timestamp.Ticks,
                SortedSetWhen.NotExists),
        };
        await Task.WhenAll(cacheSaveTasks);

        
        await hubContext
            .Clients
            .User(@event.InviterId.ToString())
            .FriendInvitationAccepted(new FriendInvitationAccepted(
                @event.InviteeId,
                @event.Timestamp));
    }
}
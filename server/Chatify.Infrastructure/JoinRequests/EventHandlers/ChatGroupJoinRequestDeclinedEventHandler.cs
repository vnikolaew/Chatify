using Chatify.Domain.Events.JoinRequests;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Messages.Hubs;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Events;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.JoinRequests.EventHandlers;

internal sealed class ChatGroupJoinRequestDeclinedEventHandler
    : IEventHandler<ChatGroupJoinRequestDeclined>
{
    private readonly IHubContext<ChatifyHub, IChatifyHubClient> _chatifyContext;
    private readonly IUserRepository _users;
    private readonly IChatGroupRepository _groups;

    public ChatGroupJoinRequestDeclinedEventHandler(
        IHubContext<ChatifyHub, IChatifyHubClient> chatifyContext,
        IUserRepository users, IChatGroupRepository groups)
    {
        _chatifyContext = chatifyContext;
        _users = users;
        _groups = groups;
    }

    public async Task HandleAsync(ChatGroupJoinRequestDeclined @event, CancellationToken cancellationToken = default)
    {
        var adminUser = await _users.GetAsync(@event.DeclinedById, cancellationToken);
        if ( adminUser is null ) return;

        await _chatifyContext
            .Clients
            .User(@event.UserId.ToString())
            .ChatGroupUserJoinRequestDeclined(new ChatGroupUserJoinRequestDeclined(
                @event.GroupId,
                @event.UserId,
                adminUser.Id,
                adminUser.Username,
                adminUser.ProfilePicture.MediaUrl,
                @event.Timestamp));
    }
}
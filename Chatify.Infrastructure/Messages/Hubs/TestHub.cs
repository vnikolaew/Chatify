using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Shared.Abstractions.Contexts;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chatify.Infrastructure.Messages.Hubs;

[Authorize]
internal sealed class TestHub : Hub
{
    private readonly IChatGroupMemberRepository _members;
    private readonly IIdentityContext _identityContext;

    public TestHub(IChatGroupMemberRepository members, IIdentityContext identityContext)
    {
        _members = members;
        _identityContext = identityContext;
    }

    public async Task<Either<Error, Unit>> JoinChatGroup(Guid chatGroupId, CancellationToken cancellationToken = default)
    {
        var isChatGroupMember = await _members.Exists(chatGroupId, _identityContext.Id, cancellationToken);
        if (!isChatGroupMember) return Error.New($"You are not a member of Chat group with Id '{chatGroupId}'.");

        var groupId = $"chat-groups:{chatGroupId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupId, cancellationToken);
        await Clients.OthersInGroup(groupId).SendAsync("UserJoined", _identityContext.Id, cancellationToken);

        return Unit.Default;
    }
    
    public async Task<Either<Error, Unit>> LeaveChatGroup(Guid chatGroupId, CancellationToken cancellationToken = default)
    {
        var isChatGroupMember = await _members.Exists(chatGroupId, _identityContext.Id, cancellationToken);
        if (!isChatGroupMember) return Error.New($"You are not a member of Chat group with Id '{chatGroupId}'.");

        var groupId = $"chat-groups:{chatGroupId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId, cancellationToken);
        await Clients.OthersInGroup(groupId).SendAsync("UserLeft", _identityContext.Id, cancellationToken);

        return Unit.Default;
    }
}

internal interface IChatifyHubClient
{
}
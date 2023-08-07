using System.Security.Claims;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Messages.Hubs.Models.Client;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Contexts;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Chatify.Infrastructure.Messages.Hubs;

using SendGroupChatMessageResult = Either<Error, Guid>;

// [Authorize]
public sealed class ChatifyHub : Hub<IChatifyHubClient>
{
    private readonly IChatGroupMemberRepository _members;
    private readonly IIdentityContext _identityContext;

    public const string Endpoint = "/chat";

    private Guid UserId => Guid.TryParse(Context.User!
        .Claims
        .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!
        .Value, out var userId)
        ? userId
        : Guid.Empty;

    public async Task SendMessage(string username, string message)
    {
        await Clients.All.ReceiveMessage(username, message);
    }

    private string Username => Context.User!
        .Claims
        .FirstOrDefault(c => c.Type == ClaimTypes.Name)!
        .Value;

    private IServiceProvider? Services => Context.GetHttpContext()?.RequestServices;

    private TService GetService<TService>()
        where TService : notnull
        => Services!.GetRequiredService<TService>();

    public ChatifyHub(IChatGroupMemberRepository members, IIdentityContext identityContext)
    {
        _members = members;
        _identityContext = identityContext;
    }

    public async Task<Either<Error, Unit>> SubscribeToAllChatGroupMessagesRequest(
        SubscribeToAllChatGroupMessagesRequest request,
        CancellationToken cancellationToken = default)
    {
        var groupIds = await GetService<IChatGroupMemberRepository>()
            .GroupsIdsByUser(UserId, cancellationToken);

        var joinGroupsTasks = groupIds.Select(id => Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"chat-groups:{id}",
            cancellationToken
        ));

        await Task.WhenAll(joinGroupsTasks);
        return Unit.Default;
    }

    public async Task<Either<Error, Unit>> SubscribeToChatGroupMessages(
        SubscribeToChatGroupMessagesRequest request,
        CancellationToken cancellationToken = default)
    {
        var isGroupMember = await GetService<IChatGroupMemberRepository>()
            .Exists(request.ChatGroupId, UserId, cancellationToken);
        if (!isGroupMember) return Error.New("");

        var groupId = $"chat-groups:{request.ChatGroupId}";
        await Groups.AddToGroupAsync(Context.ConnectionId,
            groupId,
            cancellationToken);

        return Unit.Default;
    }

    public async Task<Either<Error, Unit>> StartTypingInGroupChat(
        Guid groupId,
        DateTime timestamp,
        CancellationToken cancellationToken = default)
    {
        var isGroupMember = await GetService<IChatGroupMemberRepository>()
            .Exists(groupId, UserId, cancellationToken);
        if (!isGroupMember) return Error.New("");

        await Clients
            .Group(groupId.ToString())
            .ChatGroupMemberStartedTyping(new ChatGroupMemberStartedTyping(
                groupId,
                UserId,
                Username,
                timestamp));
        
        return Unit.Default;
    }
    
    public async Task<Either<Error, Unit>> StopTypingInGroupChat(
        Guid groupId,
        DateTime timestamp,
        CancellationToken cancellationToken = default)
    {
        var isGroupMember = await GetService<IChatGroupMemberRepository>()
            .Exists(groupId, UserId, cancellationToken);
        if (!isGroupMember) return Error.New("");

        await Clients
            .Group(groupId.ToString())
            .ChatGroupMemberStoppedTyping(new ChatGroupMemberStoppedTyping(
                groupId,
                UserId,
                Username,
                timestamp));
        
        return Unit.Default;
    }

    public async Task<Either<Error, Unit>> JoinChatGroup(
        Guid chatGroupId,
        CancellationToken cancellationToken = default)
    {
        var isChatGroupMember = await _members.Exists(chatGroupId, _identityContext.Id, cancellationToken);
        if (!isChatGroupMember) return Error.New($"You are not a member of Chat group with Id '{chatGroupId}'.");

        var groupId = $"chat-groups:{chatGroupId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupId, cancellationToken);
        // await Clients.OthersInGroup(groupId).SendAsync("UserJoined", _identityContext.Id, cancellationToken);

        return Unit.Default;
    }

    public async Task<Either<Error, Unit>> LeaveChatGroup(
        Guid chatGroupId,
        CancellationToken cancellationToken = default)
    {
        var isChatGroupMember = await _members.Exists(chatGroupId, _identityContext.Id, cancellationToken);
        if (!isChatGroupMember) return Error.New($"You are not a member of Chat group with Id '{chatGroupId}'.");

        var groupId = $"chat-groups:{chatGroupId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId, cancellationToken);
        // await Clients.OthersInGroup(groupId).SendAsync("UserLeft", _identityContext.Id, cancellationToken);

        return Unit.Default;
    }
}
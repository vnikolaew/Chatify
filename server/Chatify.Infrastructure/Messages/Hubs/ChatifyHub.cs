﻿using System.Security.Claims;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Messages.Hubs.Models.Client;
using Chatify.Infrastructure.Messages.Hubs.Models.Server;
using Chatify.Shared.Abstractions.Contexts;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Chatify.Infrastructure.Messages.Hubs;

using SendGroupChatMessageResult = Either<Error, Guid>;

// [Authorize]
public sealed class ChatifyHub(IChatGroupMemberRepository members,
        ILogger<ChatifyHub> logger, IIdentityContext identityContext)
    : Hub<IChatifyHubClient>
{
    public const string Endpoint = "/api/chat";

    public static string GetChatGroupId(Guid groupId)
        => $"chat-groups:{groupId.ToString()}";
    

    private Guid UserId => Guid.TryParse(Context.User!
        .Claims
        .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!
        .Value, out var userId)
        ? userId
        : Guid.Empty;

    public override async Task OnConnectedAsync()
    {
        var groupIds = await GetService<IChatGroupMemberRepository>()
            .GroupsIdsByUser(UserId);

        var joinGroupsTasks = groupIds
            .Chunk(10)
            .Select(ids =>
            {
                return Task.WhenAll(ids.Select(id => Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    GetChatGroupId(id))));
            });

        await Task.WhenAll(joinGroupsTasks);
        await base.OnConnectedAsync();
    }

    public async Task Test(
        string groupId,
        string value)
    {
        await Clients
            .Group(GetChatGroupId(Guid.Parse(groupId)))
            .Test(groupId, value);
    }

    public async Task SendMessage(string username,
        string message)
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

    public async Task<Either<Error, Unit>> SubscribeToAllChatGroupMessagesRequest(
        SubscribeToAllChatGroupMessagesRequest request,
        CancellationToken cancellationToken = default)
    {
        var groupIds = await GetService<IChatGroupMemberRepository>()
            .GroupsIdsByUser(UserId, cancellationToken);

        var joinGroupsTasks = groupIds.Select(id => Groups.AddToGroupAsync(
            Context.ConnectionId,
            GetChatGroupId(id),
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
        if ( !isGroupMember ) return Error.New("");

        await Groups.AddToGroupAsync(Context.ConnectionId,
            GetChatGroupId(request.ChatGroupId),
            cancellationToken);

        return Unit.Default;
    }

    [HubMethodName(nameof(StartTypingInGroupChat))]
    public async Task<Either<Error, Unit>> StartTypingInGroupChat(
        Guid groupId,
        DateTime timestamp)
    {
        var isGroupMember = await GetService<IChatGroupMemberRepository>()
            .Exists(groupId, UserId);
        if ( !isGroupMember ) return Error.New("");
        
        await Clients
            .Group(GetChatGroupId(groupId))
            .ChatGroupMemberStartedTyping(new ChatGroupMemberStartedTyping(
                groupId,
                UserId,
                Username,
                timestamp));

        return Unit.Default;
    }

    [HubMethodName(nameof(StopTypingInGroupChat))]
    public async Task<Either<Error, Unit>> StopTypingInGroupChat(
        Guid groupId,
        DateTime timestamp
      )
    {
        var isGroupMember = await GetService<IChatGroupMemberRepository>()
            .Exists(groupId, UserId);
        if ( !isGroupMember ) return Error.New("");

        await Clients
            .Group(GetChatGroupId(groupId))
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
        var isChatGroupMember = await members.Exists(chatGroupId, identityContext.Id, cancellationToken);
        if ( !isChatGroupMember ) return Error.New($"You are not a member of Chat group with Id '{chatGroupId}'.");

        await Groups.AddToGroupAsync(Context.ConnectionId, GetChatGroupId(chatGroupId), cancellationToken);
        // await Clients.OthersInGroup(groupId).SendAsync("UserJoined", _identityContext.Id, cancellationToken);

        return Unit.Default;
    }

    public async Task<Either<Error, Unit>> LeaveChatGroup(
        Guid chatGroupId,
        CancellationToken cancellationToken = default)
    {
        var isChatGroupMember = await members.Exists(chatGroupId, identityContext.Id, cancellationToken);
        if ( !isChatGroupMember ) return Error.New($"You are not a member of Chat group with Id '{chatGroupId}'.");

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetChatGroupId(chatGroupId), cancellationToken);
        // await Clients.OthersInGroup(groupId).SendAsync("UserLeft", _identityContext.Id, cancellationToken);

        return Unit.Default;
    }
}
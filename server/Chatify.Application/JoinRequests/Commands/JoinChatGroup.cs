using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.Common.Contracts;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.JoinRequests;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using OneOf;

namespace Chatify.Application.JoinRequests.Commands;

using JoinChatGroupResult = OneOf<ChatGroupNotFoundError, UserIsAlreadyGroupMemberError, Guid>;

public record JoinChatGroup(
    [Required] Guid GroupId
) : ICommand<OneOf<ChatGroupNotFoundError, UserIsAlreadyGroupMemberError, Guid>>;

internal sealed class JoinChatGroupHandler : ICommandHandler<JoinChatGroup, JoinChatGroupResult>
{
    private readonly IIdentityContext _identityContext;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IChatGroupMemberRepository _members;
    private readonly IChatGroupRepository _groups;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IDomainRepository<ChatGroupJoinRequest, Guid> _joinRequests;
    private readonly IClock _clock;

    public JoinChatGroupHandler(
        IIdentityContext identityContext,
        IChatGroupMemberRepository members,
        IChatGroupRepository groups, IGuidGenerator guidGenerator, IClock clock,
        IDomainRepository<ChatGroupJoinRequest, Guid> joinRequests, IEventDispatcher eventDispatcher)
    {
        _identityContext = identityContext;
        _members = members;
        _groups = groups;
        _guidGenerator = guidGenerator;
        _clock = clock;
        _joinRequests = joinRequests;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<JoinChatGroupResult> HandleAsync(
        JoinChatGroup command, CancellationToken cancellationToken = default)
    {
        var group = await _groups.GetAsync(command.GroupId, cancellationToken);
        if ( group is null ) return new ChatGroupNotFoundError();

        var isAlreadyMember = await _members.Exists(group.Id, _identityContext.Id, cancellationToken);
        if ( isAlreadyMember )
            return new UserIsAlreadyGroupMemberError(
                _identityContext.Id, group.Id);

        var requestId = _guidGenerator.New();
        var joinRequest = new ChatGroupJoinRequest
        {
            Id = requestId,
            UserId = _identityContext.Id,
            ChatGroupId = group.Id,
            CreatedAt = _clock.Now,
        };

        await _joinRequests.SaveAsync(joinRequest, cancellationToken);
        await _eventDispatcher.PublishAsync(new ChatGroupJoinRequested
        {
            UserId = joinRequest.UserId,
            Timestamp = _clock.Now,
            GroupId = joinRequest.ChatGroupId,
            RequestId = joinRequest.Id
        }, cancellationToken);
        
        return joinRequest.Id;
    }
}
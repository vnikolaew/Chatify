using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.JoinRequests;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Common;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using OneOf;

namespace Chatify.Application.JoinRequests.Commands;

using JoinChatGroupResult = OneOf<ChatGroupNotFoundError, UserIsAlreadyGroupMemberError, Guid>;

public record JoinChatGroup(
    [Required] Guid GroupId
) : ICommand<OneOf<ChatGroupNotFoundError, UserIsAlreadyGroupMemberError, Guid>>;

internal sealed class JoinChatGroupHandler(IIdentityContext identityContext,
        IChatGroupMemberRepository members,
        IChatGroupRepository groups,
        IGuidGenerator guidGenerator,
        IClock clock,
        IDomainRepository<ChatGroupJoinRequest, Guid> joinRequests,
        IEventDispatcher eventDispatcher)
    : ICommandHandler<JoinChatGroup, JoinChatGroupResult>
{
    public async Task<JoinChatGroupResult> HandleAsync(
        JoinChatGroup command, CancellationToken cancellationToken = default)
    {
        var group = await groups.GetAsync(command.GroupId, cancellationToken);
        if ( group is null ) return new ChatGroupNotFoundError();

        var isAlreadyMember = await members.Exists(group.Id, identityContext.Id, cancellationToken);
        if ( isAlreadyMember )
            return new UserIsAlreadyGroupMemberError(
                identityContext.Id, group.Id);

        var requestId = guidGenerator.New();
        var joinRequest = new ChatGroupJoinRequest
        {
            Id = requestId,
            UserId = identityContext.Id,
            ChatGroupId = group.Id,
            CreatedAt = clock.Now,
        };

        await joinRequests.SaveAsync(joinRequest, cancellationToken);
        await eventDispatcher.PublishAsync(new ChatGroupJoinRequested
        {
            UserId = joinRequest.UserId,
            Timestamp = clock.Now,
            GroupId = joinRequest.ChatGroupId,
            RequestId = joinRequest.Id
        }, cancellationToken);
        
        return joinRequest.Id;
    }
}
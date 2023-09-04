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
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.JoinRequests.Commands;

using AcceptChatGroupJoinRequestResult = OneOf<ChatGroupNotFoundError, UserIsNotGroupAdminError, Error, Guid>;

public record AcceptChatGroupJoinRequest(
    [Required] Guid RequestId
) : ICommand<AcceptChatGroupJoinRequestResult>;

internal sealed class AcceptChatGroupJoinRequestHandler(IChatGroupMemberRepository members,
        IIdentityContext identityContext,
        IDomainRepository<ChatGroupJoinRequest, Guid> joinRequests,
        IChatGroupRepository groups,
        IGuidGenerator guidGenerator,
        IClock clock,
        IEventDispatcher eventDispatcher,
        IUserRepository users)
    : ICommandHandler<AcceptChatGroupJoinRequest, AcceptChatGroupJoinRequestResult>
{
    public async Task<AcceptChatGroupJoinRequestResult> HandleAsync(
        AcceptChatGroupJoinRequest command,
        CancellationToken cancellationToken = default)
    {
        var request = await joinRequests.GetAsync(command.RequestId, cancellationToken);
        if ( request is null ) return Error.New("");

        var group = await groups.GetAsync(request.ChatGroupId, cancellationToken);
        if ( group is null ) return new ChatGroupNotFoundError();

        var isCurrentUserGroupAdmin = group
            .AdminIds
            .Any(_ => _ == identityContext.Id);
        if ( !isCurrentUserGroupAdmin ) return new UserIsNotGroupAdminError(identityContext.Id, group.Id);

        var user = await users.GetAsync(request.UserId, cancellationToken);

        var membershipId = guidGenerator.New();
        var groupMember = new ChatGroupMember
        {
            Id = membershipId,
            ChatGroupId = group.Id,
            UserId = user!.Id,
            Username = user.Username,
            CreatedAt = clock.Now,
            User = user
        };

        await members.SaveAsync(groupMember, cancellationToken);
        await eventDispatcher.PublishAsync(new ChatGroupJoinRequestAccepted
        {
            RequestId = request.Id,
            UserId = request.UserId,
            AcceptedById = identityContext.Id,
            Timestamp = clock.Now,
            Username = user.Username,
            GroupId = group.Id
        }, cancellationToken);

        return groupMember.Id;
    }
}
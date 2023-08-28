using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Models;
using Chatify.Application.User.Common;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Groups;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using OneOf;

namespace Chatify.Application.ChatGroups.Commands;

using AddChatGroupMemberResult =
    OneOf<UserNotFound, UserIsNotGroupAdminError, ChatGroupNotFoundError, UserIsAlreadyGroupMemberError, Guid>;

public record UserIsAlreadyGroupMemberError(Guid UserId, Guid ChatGroupId)
    : BaseError("User is already a member of this chat group.");

public record AddChatGroupMember(
    [Required] Guid GroupId,
    [Required] Guid NewMemberId,
    // TODO: Refactor to strongly typed object:
    sbyte MembershipType
) : ICommand<AddChatGroupMemberResult>;

internal sealed class AddChatGroupMemberHandler(
        IIdentityContext identityContext,
        IChatGroupMemberRepository members,
        IDomainRepository<ChatGroup, Guid> groups,
        IEventDispatcher eventDispatcher,
        IClock clock, IDomainRepository<Domain.Entities.User, Guid> users)
    : ICommandHandler<AddChatGroupMember, AddChatGroupMemberResult>
{
    public async Task<AddChatGroupMemberResult> HandleAsync(
        AddChatGroupMember command,
        CancellationToken cancellationToken = default)
    {
        var group = await groups.GetAsync(command.GroupId, cancellationToken);

        if ( group is null ) return new ChatGroupNotFoundError();
        if ( !group.AdminIds.Contains(identityContext.Id) )
        {
            return new UserIsNotGroupAdminError(identityContext.Id, group.Id);
        }

        var memberUser = await users.GetAsync(command.NewMemberId, cancellationToken);
        if ( memberUser is null )
        {
            return new UserNotFound();
        }

        var isMember = await members.Exists(group.Id, memberUser.Id, cancellationToken);
        if ( isMember )
        {
            return new UserIsAlreadyGroupMemberError(memberUser.Id, group.Id);
        }

        var member = new ChatGroupMember
        {
            Id = Guid.NewGuid(),
            Username = memberUser.Username,
            UserId = memberUser.Id,
            User = memberUser,
            ChatGroup = group,
            MembershipType = command.MembershipType
        };

        await members.SaveAsync(member, cancellationToken);
        await eventDispatcher.PublishAsync(new ChatGroupMemberAddedEvent
        {
            GroupId = group.Id,
            AddedById = identityContext.Id,
            AddedByUsername = identityContext.Username,
            MemberId = member.Id,
            MembershipType = command.MembershipType,
            Timestamp = clock.Now
        }, cancellationToken);

        return member.Id;
    }
}
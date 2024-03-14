using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Contracts;
using Chatify.Application.Common;
using Chatify.Application.Common.Models;
using Chatify.Application.User.Common;
using Chatify.Application.User.Contracts;
using Chatify.Domain.Events.Groups;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt.Common;
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
    IChatGroupsService chatGroupsService,
    IUsersService usersService,
    IIdentityContext identityContext,
    IEventDispatcher eventDispatcher,
    IClock clock)
    : BaseCommandHandler<AddChatGroupMember, AddChatGroupMemberResult>(eventDispatcher, identityContext, clock)
{
    public override async Task<AddChatGroupMemberResult> HandleAsync(
        AddChatGroupMember command,
        CancellationToken cancellationToken = default)
    {
        var newMember = await usersService.GetById(command.NewMemberId, cancellationToken);
        if ( newMember is null ) return new UserNotFound();

        var response = ( await chatGroupsService.AddChatGroupMembersAsync(
                new AddChatGroupMembersRequest(command.GroupId,
                [
                    new AddChatGroupMemberRequest(
                        newMember.Id,
                        newMember.Username,
                        command.MembershipType)
                ]),
                cancellationToken) )
            .FirstOrDefault();

        if ( response.Value is Error error ) return Guid.Empty;
        var newMemberId = response.AsT1;

        await eventDispatcher.PublishAsync(new ChatGroupMemberAddedEvent
        {
            GroupId = command.GroupId,
            AddedById = identityContext.Id,
            AddedByUsername = identityContext.Username,
            MemberId = newMemberId,
            MembershipType = command.MembershipType,
            Timestamp = clock.Now
        }, cancellationToken);

        return newMemberId;
    }
}
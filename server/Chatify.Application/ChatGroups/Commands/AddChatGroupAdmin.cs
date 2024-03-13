using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common;
using Chatify.Application.Common.Models;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Groups;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using OneOf;

namespace Chatify.Application.ChatGroups.Commands;

using AddChatGroupAdminResult = OneOf<ChatGroupNotFoundError, UserIsNotMemberError, UserIsNotGroupAdminError, Unit>;

public record ChatGroupNotFoundError;

public record UserIsNotMemberError(Guid UserId, Guid ChatGroupId)
    : BaseError("User is not a member of this chat group.");

public record UserIsNotGroupAdminError(Guid UserId, Guid ChatGroupId)
    : BaseError("User is not an admin of this chat group.");

public record AddChatGroupAdmin(
    [Required] Guid ChatGroupId,
    [Required] Guid NewAdminId
) : ICommand<AddChatGroupAdminResult>;

internal sealed class AddChatGroupAdminHandler(
    IDomainRepository<ChatGroup, Guid> groups,
    IEventDispatcher eventDispatcher,
    IIdentityContext identityContext,
    IClock clock,
    IChatGroupMemberRepository members)
    : BaseCommandHandler<AddChatGroupAdmin, AddChatGroupAdminResult>(eventDispatcher, identityContext, clock)
{
    public override async Task<AddChatGroupAdminResult> HandleAsync(
        AddChatGroupAdmin command,
        CancellationToken cancellationToken = default)
    {
        var chatGroup = await groups.GetAsync(command.ChatGroupId, cancellationToken);
        if ( chatGroup is null ) return new ChatGroupNotFoundError();

        var isMember = await members.Exists(chatGroup.Id, identityContext.Id, cancellationToken);
        if ( !isMember ) return new UserIsNotMemberError(identityContext.Id, command.ChatGroupId);

        if ( !chatGroup.HasAdmin(identityContext.Id) )
            return new UserIsNotGroupAdminError(identityContext.Id, command.ChatGroupId);

        if ( chatGroup.HasAdmin(command.NewAdminId) )
            return new UserIsNotGroupAdminError(identityContext.Id, command.ChatGroupId);

        await groups.UpdateAsync(chatGroup, group =>
        {
            group.AddAdmin(command.NewAdminId);
            group.UpdatedAt = clock.Now;
        }, cancellationToken);

        await eventDispatcher.PublishAsync(new ChatGroupAdminAdded
        {
            GroupId = chatGroup.Id,
            AdminId = command.NewAdminId,
            Timestamp = clock.Now
        }, cancellationToken);

        return Unit.Default;
    }
}
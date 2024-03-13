using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using OneOf;

namespace Chatify.Application.ChatGroups.Commands;

using RemoveChatGroupAdminResult = OneOf<ChatGroupNotFoundError, UserIsNotMemberError, UserIsNotGroupAdminError, Unit>;

public record RemoveChatGroupAdmin(
    [Required] Guid ChatGroupId,
    [Required] Guid AdminId
) : ICommand<RemoveChatGroupAdminResult>;

internal sealed class RemoveChatGroupAdminHandler(
    IDomainRepository<ChatGroup, Guid> groups,
    IEventDispatcher eventDispatcher,
    IIdentityContext identityContext,
    IClock clock,
    IChatGroupMemberRepository members
) : BaseCommandHandler<RemoveChatGroupAdmin, RemoveChatGroupAdminResult>(eventDispatcher, identityContext, clock)
{
    public override async Task<RemoveChatGroupAdminResult> HandleAsync(
        RemoveChatGroupAdmin command,
        CancellationToken cancellationToken = default)
    {
        var chatGroup = await groups.GetAsync(command.ChatGroupId, cancellationToken);
        if ( chatGroup is null ) return new ChatGroupNotFoundError();

        var isMember = await members.Exists(chatGroup.Id, identityContext.Id, cancellationToken);
        if ( !isMember ) return new UserIsNotMemberError(identityContext.Id, command.ChatGroupId);

        if ( chatGroup.CreatorId != identityContext.Id )
            return new UserIsNotMemberError(identityContext.Id, chatGroup.Id);

        if ( !chatGroup.HasAdmin(command.AdminId) )
            return new UserIsNotGroupAdminError(command.AdminId, command.ChatGroupId);

        await groups.UpdateAsync(chatGroup, group =>
        {
            group.RemoveAdmin(command.AdminId);
            group.UpdatedAt = clock.Now;
        }, cancellationToken);

        return Unit.Default;
    }
}
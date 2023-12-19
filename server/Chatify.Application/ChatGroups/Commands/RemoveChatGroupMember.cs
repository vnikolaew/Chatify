using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common;
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

using RemoveChatGroupMemberResult = OneOf<ChatGroupNotFoundError, UserIsNotMemberError, UserIsNotGroupAdminError, Unit>;

public record RemoveChatGroupMember(
    [Required] Guid GroupId,
    [Required] Guid MemberId
) : ICommand<RemoveChatGroupMemberResult>;

internal sealed class RemoveChatGroupMemberHandler(
    IDomainRepository<ChatGroup, Guid> groups,
    IChatGroupMemberRepository members,
    IIdentityContext identityContext,
    IEventDispatcher eventDispatcher,
    IClock clock)
    : BaseCommandHandler<RemoveChatGroupMember, RemoveChatGroupMemberResult>(eventDispatcher, identityContext, clock)
{
    public override async Task<RemoveChatGroupMemberResult> HandleAsync(
        RemoveChatGroupMember command,
        CancellationToken cancellationToken = default)
    {
        var chatGroup = await groups.GetAsync(command.GroupId, cancellationToken);
        if ( chatGroup is null ) return new ChatGroupNotFoundError();

        if ( chatGroup.AdminIds.All(id => id != identityContext.Id) )
        {
            return new UserIsNotGroupAdminError(identityContext.Id, chatGroup.Id);
        }

        var memberExists = await members.Exists(
            command.GroupId, command.MemberId,
            cancellationToken);
        if ( !memberExists ) return new UserIsNotMemberError(identityContext.Id, chatGroup.Id);

        await members.DeleteAsync(identityContext.Id, cancellationToken);
        await eventDispatcher.PublishAsync(new ChatGroupMemberRemovedEvent
        {
            GroupId = chatGroup.Id,
            Timestamp = clock.Now,
            MemberId = identityContext.Id,
            RemovedById = identityContext.Id,
        }, cancellationToken);

        return Unit.Default;
    }
}
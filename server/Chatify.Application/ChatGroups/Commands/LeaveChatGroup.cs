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

using LeaveChatGroupResult = OneOf<ChatGroupNotFoundError, UserIsNotMemberError, Unit>;

public record LeaveChatGroup(
    [Required] Guid GroupId,
    [MinLength(3), MaxLength(200)] string? Reason
) : ICommand<LeaveChatGroupResult>;

internal sealed class LeaveChatGroupHandler(
    IChatGroupMemberRepository members,
    IClock clock,
    IDomainRepository<ChatGroup, Guid> groups,
    IIdentityContext identityContext,
    IEventDispatcher eventDispatcher)
    : BaseCommandHandler<LeaveChatGroup, LeaveChatGroupResult>(eventDispatcher, identityContext, clock)
{
    public override async Task<LeaveChatGroupResult> HandleAsync(
        LeaveChatGroup command,
        CancellationToken cancellationToken = default)
    {
        var group = await groups.GetAsync(command.GroupId, cancellationToken);
        if ( group is null ) return new ChatGroupNotFoundError();

        var member = await members.ByGroupAndUser(command.GroupId, identityContext.Id, cancellationToken);
        if ( member is null ) return new UserIsNotMemberError(identityContext.Id, group.Id);

        var success = await members.DeleteAsync(member.Id, cancellationToken);
        if ( !success ) return Unit.Default;

        // TODO: Fire an event:
        await eventDispatcher.PublishAsync(new ChatGroupMemberLeftEvent
        {
            UserId = identityContext.Id,
            GroupId = group.Id,
            Timestamp = clock.Now,
            Reason = command.Reason
        }, cancellationToken);

        return Unit.Default;
    }
}
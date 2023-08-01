using System.ComponentModel.DataAnnotations;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Events.Groups;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Events;
using Chatify.Shared.Abstractions.Time;
using LanguageExt;
using LanguageExt.Common;

namespace Chatify.Application.ChatGroups.Commands;

using LeaveChatGroupResult = Either<Error, Unit>;

public record LeaveChatGroup(
    [Required] Guid GroupId,
    [MinLength(3), MaxLength(200)] string? Reason
) : ICommand<LeaveChatGroupResult>;

internal sealed class LeaveChatGroupHandler : ICommandHandler<LeaveChatGroup, LeaveChatGroupResult>
{
    private readonly IChatGroupMemberRepository _members;
    private readonly IDomainRepository<ChatGroup, Guid> _groups;
    private readonly IIdentityContext _identityContext;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IClock _clock;

    public LeaveChatGroupHandler(
        IChatGroupMemberRepository members,
        IClock clock, IDomainRepository<ChatGroup, Guid> groups,
        IIdentityContext identityContext,
        IEventDispatcher eventDispatcher)
    {
        _members = members;
        _clock = clock;
        _groups = groups;
        _identityContext = identityContext;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<LeaveChatGroupResult> HandleAsync(
        LeaveChatGroup command,
        CancellationToken cancellationToken = default)
    {
        var group = await _groups.GetAsync(command.GroupId, cancellationToken);
        if (group is null) return Error.New("");

        var member = await _members.ByGroupAndUser(
            command.GroupId,
            _identityContext.Id, cancellationToken);
        if (member is null) return Error.New("");

        var success = await _members.DeleteAsync(member.Id, cancellationToken);
        
        // TODO: Fire an event:
        await _eventDispatcher.PublishAsync(new ChatGroupMemberLeftEvent
        {
            UserId = _identityContext.Id,
            GroupId = group.Id,
            Timestamp = _clock.Now,
            Reason = command.Reason
        }, cancellationToken);
        
        return success ? Unit.Default : Error.New("");
    }
}
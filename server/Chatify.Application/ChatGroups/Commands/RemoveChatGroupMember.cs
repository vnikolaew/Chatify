using System.ComponentModel.DataAnnotations;
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

internal sealed class RemoveChatGroupMemberHandler
    : ICommandHandler<RemoveChatGroupMember, RemoveChatGroupMemberResult>
{
    private readonly IDomainRepository<ChatGroup, Guid> _groups;
    private readonly IChatGroupMemberRepository _members;
    private readonly IIdentityContext _identityContext;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IClock _clock;

    public RemoveChatGroupMemberHandler(
        IDomainRepository<ChatGroup, Guid> groups,
        IChatGroupMemberRepository members,
        IIdentityContext identityContext,
        IEventDispatcher eventDispatcher,
        IClock clock)
    {
        _groups = groups;
        _members = members;
        _identityContext = identityContext;
        _eventDispatcher = eventDispatcher;
        _clock = clock;
    }

    public async Task<RemoveChatGroupMemberResult> HandleAsync(
        RemoveChatGroupMember command,
        CancellationToken cancellationToken = default)
    {
        var chatGroup = await _groups.GetAsync(command.GroupId, cancellationToken);
        if ( chatGroup is null ) return new ChatGroupNotFoundError();

        if ( chatGroup.AdminIds.All(id => id != _identityContext.Id) )
        {
            return new UserIsNotGroupAdminError(_identityContext.Id, chatGroup.Id);
        }

        var memberExists = await _members.Exists(
            command.GroupId, command.MemberId,
            cancellationToken);
        if ( !memberExists ) return new UserIsNotMemberError(_identityContext.Id, chatGroup.Id);

        await _members.DeleteAsync(_identityContext.Id, cancellationToken);
        await _eventDispatcher.PublishAsync(new ChatGroupMemberRemovedEvent
        {
            GroupId = chatGroup.Id,
            Timestamp = _clock.Now,
            MemberId = _identityContext.Id,
            RemovedById = _identityContext.Id,
        }, cancellationToken);

        return Unit.Default;
    }
}
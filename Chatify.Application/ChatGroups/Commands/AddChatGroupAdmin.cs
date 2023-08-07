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

using AddChatGroupAdminResult = OneOf<ChatGroupNotFoundError, UserIsNotMemberError, UserIsNotGroupAdminError, Unit>;

public record ChatGroupNotFoundError;

public record UserIsNotMemberError(Guid UserId, Guid ChatGroupId);

public record UserIsNotGroupAdminError(Guid UserId, Guid ChatGroupId);

public record AddChatGroupAdmin(
    [Required] Guid ChatGroupId,
    [Required] Guid NewAdminId
) : ICommand<AddChatGroupAdminResult>;

internal sealed class AddChatGroupAdminHandler : ICommandHandler<AddChatGroupAdmin, AddChatGroupAdminResult>
{
    private readonly IDomainRepository<ChatGroup, Guid> _groups;
    private readonly IChatGroupMemberRepository _members;
    private readonly IClock _clock;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IIdentityContext _identityContext;

    public AddChatGroupAdminHandler(
        IDomainRepository<ChatGroup, Guid> groups,
        IEventDispatcher eventDispatcher,
        IIdentityContext identityContext,
        IClock clock, IChatGroupMemberRepository members)
    {
        _groups = groups;
        _eventDispatcher = eventDispatcher;
        _identityContext = identityContext;
        _clock = clock;
        _members = members;
    }

    public async Task<AddChatGroupAdminResult> HandleAsync(
        AddChatGroupAdmin command,
        CancellationToken cancellationToken = default)
    {
        var chatGroup = await _groups.GetAsync(command.ChatGroupId, cancellationToken);
        if ( chatGroup is null ) return new ChatGroupNotFoundError();

        var isMember = await _members.Exists(chatGroup.Id, _identityContext.Id, cancellationToken);
        if ( !isMember ) return new UserIsNotMemberError(_identityContext.Id, command.ChatGroupId);

        if ( !chatGroup.AdminIds.Contains(_identityContext.Id) )
            return new UserIsNotGroupAdminError(_identityContext.Id, command.ChatGroupId);

        if ( chatGroup.AdminIds.Contains(command.NewAdminId) )
            return new UserIsNotGroupAdminError(_identityContext.Id, command.ChatGroupId);

        await _groups.UpdateAsync(chatGroup.Id, group =>
        {
            group.AdminIds.Add(command.NewAdminId);
            group.UpdatedAt = _clock.Now;
        }, cancellationToken);

        await _eventDispatcher.PublishAsync(new ChatGroupAdminAdded
        {
            GroupId = chatGroup.Id,
            AdminId = command.NewAdminId,
            Timestamp = _clock.Now
        }, cancellationToken);

        return Unit.Default;
    }
}
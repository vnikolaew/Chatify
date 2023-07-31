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

using AddChatGroupMemberResult = Either<Error, Guid>;

public record AddChatGroupMember(
    [Required] Guid GroupId,
    [Required] Guid NewMemberId,
    // TODO: Refactor to strongly typed object:
    sbyte MembershipType
) : ICommand<AddChatGroupMemberResult>;

internal sealed class AddChatGroupMemberHandler : ICommandHandler<AddChatGroupMember, AddChatGroupMemberResult>
{
    private readonly IIdentityContext _identityContext;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IDomainRepository<ChatGroupMember, Guid> _members;
    private readonly IDomainRepository<ChatGroup, Guid> _groups;
    private readonly IClock _clock;


    public AddChatGroupMemberHandler(
        IIdentityContext identityContext,
        IDomainRepository<ChatGroupMember, Guid> members,
        IDomainRepository<ChatGroup, Guid> groups,
        IEventDispatcher eventDispatcher,
        IClock clock)
    {
        _identityContext = identityContext;
        _members = members;
        _groups = groups;
        _eventDispatcher = eventDispatcher;
        _clock = clock;
    }

    public async Task<AddChatGroupMemberResult> HandleAsync(
        AddChatGroupMember command,
        CancellationToken cancellationToken = default)
    {
        var group = await _groups.GetAsync(command.GroupId, cancellationToken);

        if (group is null) return Error.New("Chat group does not exist.");
        if (!group.AdminIds.Contains(_identityContext.Id))
        {
            return Error.New("Current user does not have permissions to add new members.");
        }

        var member = new ChatGroupMember
        {
            Id = Guid.NewGuid(),
            UserId = command.NewMemberId,
            ChatGroupId = group.Id,
            MembershipType = command.MembershipType
        };

        await _members.SaveAsync(member, cancellationToken);
        await _eventDispatcher.PublishAsync(new ChatGroupMemberAddedEvent
        {
            GroupId = group.Id,
            MemberId = member.Id,
            MembershipType = command.MembershipType,
            Timestamp = _clock.Now
        }, cancellationToken);
            
        return member.Id;
    }
}
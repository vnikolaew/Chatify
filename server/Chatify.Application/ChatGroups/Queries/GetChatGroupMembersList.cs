using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Application.Common.Behaviours.Timing;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using OneOf;

namespace Chatify.Application.ChatGroups.Queries;

using GetChatGroupMembersListResult = OneOf<UserIsNotMemberError, ChatGroupNotFoundError, List<Domain.Entities.User>>;

[Cached("chat-group-members", 30)]
[Timed]
public record GetChatGroupMembersList(
    [Required] [property: CacheKey] Guid ChatGroupId
) : IQuery<GetChatGroupMembersListResult>;

internal sealed class GetChatGroupMembersListHandler
    : IQueryHandler<GetChatGroupMembersList, GetChatGroupMembersListResult>
{
    private readonly IChatGroupMemberRepository _members;
    private readonly IDomainRepository<ChatGroup, Guid> _groups;
    private readonly IUserRepository _users;
    private readonly IIdentityContext _identityContext;

    public GetChatGroupMembersListHandler(
        IChatGroupMemberRepository members,
        IDomainRepository<ChatGroup, Guid> groups,
        IIdentityContext identityContext,
        IUserRepository users)
    {
        _members = members;
        _groups = groups;
        _identityContext = identityContext;
        _users = users;
    }

    public async Task<GetChatGroupMembersListResult> HandleAsync(
        GetChatGroupMembersList command,
        CancellationToken cancellationToken = default)
    {
        var group = await _groups.GetAsync(
            command.ChatGroupId,
            cancellationToken);
        if ( group is null ) return new ChatGroupNotFoundError();

        var isMember = await _members.Exists(
            group.Id, _identityContext.Id, cancellationToken);
        if ( !isMember ) return new UserIsNotMemberError(_identityContext.Id, group.Id);

        var memberIds = await _members
            .UserIdsByGroup(group.Id, cancellationToken);

        var users = await _users.GetByIds(memberIds!, cancellationToken)
                    ?? new List<Domain.Entities.User>();
        return users;
    }
}
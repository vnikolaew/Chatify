using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Application.Common.Behaviours.Timing;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using Chatify.Shared.Infrastructure.Common.Extensions;
using OneOf;

namespace Chatify.Application.ChatGroups.Queries;

using GetChatGroupDetailsResult = OneOf<ChatGroupNotFoundError, UserIsNotMemberError, ChatGroupDetailsEntry>;
using User = Domain.Entities.User;

public record ChatGroupDetailsEntry(
    ChatGroup ChatGroup,
    List<User> Members,
    User Creator);

[Cached("chat-group-details", 60)]
public record GetChatGroupDetails(
    [Required] [property: CacheKey] Guid GroupId
) : IQuery<GetChatGroupDetailsResult>;

[Timed]
internal sealed class GetChatGroupDetailsHandler
    : IQueryHandler<GetChatGroupDetails, GetChatGroupDetailsResult>
{
    private readonly IIdentityContext _identityContext;
    private readonly IUserRepository _users;
    private readonly IChatGroupRepository _groups;
    private readonly IChatGroupMemberRepository _members;

    public GetChatGroupDetailsHandler(IIdentityContext identityContext, IChatGroupMemberRepository members,
        IChatGroupRepository groups, IUserRepository users)
    {
        _identityContext = identityContext;
        _members = members;
        _groups = groups;
        _users = users;
    }

    public async Task<GetChatGroupDetailsResult> HandleAsync(
        GetChatGroupDetails query, CancellationToken cancellationToken = default)
    {
        var group = await _groups.GetAsync(query.GroupId, cancellationToken);
        if ( group is null ) return new ChatGroupNotFoundError();


        var isMember = await _members.Exists(query.GroupId, _identityContext.Id, cancellationToken);
        if ( !isMember ) return new UserIsNotMemberError(_identityContext.Id, group.Id);
        
        var adminTask = _users.GetAsync(group.CreatorId, cancellationToken);
        var groupMembersTask = _members.ByGroup(group.Id, cancellationToken);
        var (admin, groupMembers ) = await ( adminTask, groupMembersTask ).WhenAll();
            
        var members = await _users.GetByIds(groupMembers!.Select(_ => _.UserId), cancellationToken);
        return new ChatGroupDetailsEntry(group, members!, admin!);
    }
}
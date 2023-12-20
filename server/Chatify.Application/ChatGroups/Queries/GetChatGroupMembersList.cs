using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.Common;
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

internal sealed class GetChatGroupMembersListHandler(
    IChatGroupMemberRepository members,
    IDomainRepository<ChatGroup, Guid> groups,
    IIdentityContext identityContext,
    IUserRepository users)
    : BaseQueryHandler<GetChatGroupMembersList, GetChatGroupMembersListResult>(identityContext)
{
    public override async Task<GetChatGroupMembersListResult> HandleAsync(GetChatGroupMembersList command,
        CancellationToken cancellationToken = default)
    {
        var group = await groups.GetAsync(
            command.ChatGroupId,
            cancellationToken);
        if ( group is null ) return new ChatGroupNotFoundError();

        var isMember = await members.Exists(
            group.Id, identityContext.Id, cancellationToken);
        if ( !isMember ) return new UserIsNotMemberError(identityContext.Id, group.Id);

        var memberIds = await members
            .UserIdsByGroup(group.Id, cancellationToken);

        var memberUsers = await users.GetByIds(memberIds!, cancellationToken)
                          ?? new List<Domain.Entities.User>();
        return memberUsers;
    }
}
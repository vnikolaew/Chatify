using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.ChatGroups.Contracts;
using Chatify.Application.Common;
using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Application.Common.Behaviours.Timing;
using Chatify.Application.User.Contracts;
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
    IChatGroupsService chatGroupsService,
    IUsersService usersService,
    IIdentityContext identityContext)
    : BaseQueryHandler<GetChatGroupMembersList, GetChatGroupMembersListResult>(identityContext)
{
    public override async Task<GetChatGroupMembersListResult> HandleAsync(GetChatGroupMembersList command,
        CancellationToken cancellationToken = default)
    {
        var memberIds = await chatGroupsService.GetChatGroupMemberIdsAsync(command.ChatGroupId, cancellationToken);
        if ( memberIds.IsT0 ) return new ChatGroupNotFoundError();

        var memberUsers = await usersService.GetByIds(memberIds.AsT1!, cancellationToken);
        return memberUsers;
    }
}
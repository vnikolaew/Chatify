using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.ChatGroups.Contracts;
using Chatify.Application.Common;
using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Application.Common.Behaviours.Timing;
using Chatify.Application.User.Contracts;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using Chatify.Shared.Infrastructure.Common.Extensions;
using LanguageExt.Common;
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
internal sealed class GetChatGroupDetailsHandler(
    IChatGroupsService chatGroupsService,
    IUsersService usersService,
    IIdentityContext identityContext,
    IChatGroupMemberRepository members
)
    : BaseQueryHandler<GetChatGroupDetails, GetChatGroupDetailsResult>(identityContext)
{
    public override async Task<GetChatGroupDetailsResult> HandleAsync(
        GetChatGroupDetails query,
        CancellationToken cancellationToken = default)
    {
        var groupDetails = await chatGroupsService.GetChatGroupDetails(query.GroupId, cancellationToken);
        if ( groupDetails.Value is Error ) return new ChatGroupNotFoundError();

        var group = groupDetails.AsT1;
        var (admin, groupMembers) = await (
            usersService.GetById(group.CreatorId, cancellationToken),
            members.ByGroup(group.Id, cancellationToken) );

        var membersUsers = await usersService
            .GetByIds(groupMembers!.Select(_ => _.UserId), cancellationToken);
        return new ChatGroupDetailsEntry(group, membersUsers, admin!);
    }
}
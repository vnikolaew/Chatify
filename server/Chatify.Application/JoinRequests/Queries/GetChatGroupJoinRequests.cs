using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Application.Common.Behaviours.Timing;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.JoinRequests.Queries;

using GetChatGroupJoinRequestsResult =
    OneOf<ChatGroupNotFoundError, UserIsNotGroupAdminError, List<GroupJoinRequestEntry>>;

public record GroupJoinRequestEntry(
    ChatGroupJoinRequest JoinRequest,
    Domain.Entities.User User);

[Cached("chat-group-join-requests", 60)]
public record GetChatGroupJoinRequests(
    [Required] [property: CacheKey] Guid GroupId
) : IQuery<GetChatGroupJoinRequestsResult>;

[Timed]
internal sealed class GetChatGroupJoinRequestsHandler(IChatGroupJoinRequestRepository joinRequests,
        IIdentityContext identityContext,
        IChatGroupRepository groups,
        IUserRepository users)
    : IQueryHandler<GetChatGroupJoinRequests, GetChatGroupJoinRequestsResult>
{
    public async Task<GetChatGroupJoinRequestsResult> HandleAsync(GetChatGroupJoinRequests query,
        CancellationToken cancellationToken = default)
    {
        var group = await groups.GetAsync(query.GroupId, cancellationToken);
        if ( group is null ) return new ChatGroupNotFoundError();

        var isCurrentUserAdmin = group.AdminIds.Contains(identityContext.Id);
        if ( !isCurrentUserAdmin ) return new UserIsNotGroupAdminError(identityContext.Id, group.Id);

        var requests = await joinRequests
            .ByGroup(group.Id, cancellationToken);

        var userInfos = await users.GetByIds(
            requests.Select(r => r.UserId), cancellationToken);

        return requests
            .Zip(userInfos!,
                (request, user) => new GroupJoinRequestEntry(request, user))
            .ToList();
    }
}
using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Application.Common.Behaviours.Timing;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.JoinRequests.Queries;

using GetChatGroupJoinRequestsResult = OneOf<Error, List<GroupJoinRequestEntry>>;

public record GroupJoinRequestEntry(
    ChatGroupJoinRequest JoinRequest,
    Domain.Entities.User User);

[Cached("chat-group-join-requests", 60)]
public record GetChatGroupJoinRequests(
    [Required] [property: CacheKey] Guid GroupId
) : IQuery<GetChatGroupJoinRequestsResult>;

[Timed]
internal sealed class GetChatGroupJoinRequestsHandler
    : IQueryHandler<GetChatGroupJoinRequests, GetChatGroupJoinRequestsResult>
{
    private readonly IChatGroupJoinRequestRepository _joinRequests;
    private readonly IUserRepository _users;
    private readonly IIdentityContext _identityContext;
    private readonly IChatGroupRepository _groups;

    public GetChatGroupJoinRequestsHandler(
        IChatGroupJoinRequestRepository joinRequests,
        IIdentityContext identityContext,
        IChatGroupRepository groups)
    {
        _joinRequests = joinRequests;
        _identityContext = identityContext;
        _groups = groups;
    }

    public async Task<GetChatGroupJoinRequestsResult> HandleAsync(
        GetChatGroupJoinRequests query,
        CancellationToken cancellationToken = default)
    {
        var group = await _groups.GetAsync(query.GroupId, cancellationToken);
        if ( group is null ) return Error.New("");

        var isCurrentUserAdmin = group.AdminIds.Any(_ => _ == _identityContext.Id);
        if ( !isCurrentUserAdmin ) return Error.New("");

        var requests = await _joinRequests
            .ByGroup(group.Id, cancellationToken);

        var userInfos = await _users.GetByIds(
            requests.Select(r => r.UserId), cancellationToken);

        return requests
            .Zip(userInfos!,
                (request, user) => new GroupJoinRequestEntry(request, user))
            .ToList();
    }
}
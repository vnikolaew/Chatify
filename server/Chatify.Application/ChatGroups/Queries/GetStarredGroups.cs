using Chatify.Application.Common;
using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Application.User.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;

namespace Chatify.Application.ChatGroups.Queries;

using GetStarredGroupsResult = OneOf.OneOf<UserNotFound, List<ChatGroup>>;

[CachedByUser("starred-groups", 60 * 5)]
public record GetStarredGroups : IQuery<GetStarredGroupsResult>;

internal sealed class GetStarredGroupsHandler(
    IIdentityContext identityContext,
    IUserRepository users,
    IChatGroupRepository groups)
    : BaseQueryHandler<GetStarredGroups, GetStarredGroupsResult>(identityContext)
{
    public override async Task<GetStarredGroupsResult> HandleAsync(GetStarredGroups command,
        CancellationToken cancellationToken = default)
    {
        var user = await users.GetAsync(identityContext.Id, cancellationToken);
        if ( user is null ) return new UserNotFound();

        var userGroups = await groups.GetByIds(user!.StarredChatGroups, cancellationToken);
        return userGroups.Where(_ => _ is not null).ToList();
    }
}
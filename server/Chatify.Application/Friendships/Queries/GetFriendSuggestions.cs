using Chatify.Application.Common;
using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using Chatify.Shared.Infrastructure.Common.Extensions;

namespace Chatify.Application.Friendships.Queries;

[Cached(queryCacheKeyPrefix: "friend-suggestions", 60)]
public record GetFriendSuggestions : IQuery<List<Domain.Entities.User>>;

internal sealed class GetFriendSuggestionsHandler(
    IIdentityContext identityContext,
    IUserRepository users,
    IFriendInvitationRepository invites,
    IFriendshipsRepository friendships,
    IChatGroupMemberRepository members)
    :
        BaseQueryHandler<GetFriendSuggestions, List<Domain.Entities.User>>(identityContext)
{
    public override async Task<List<Domain.Entities.User>> HandleAsync(GetFriendSuggestions command,
        CancellationToken cancellationToken = default)
    {
        // Get user's friends:
        var (friendIds, userInviteeIds) = (
            ( await friendships.AllForUser(IdentityContext.Id, cancellationToken) )
            .Select(u => u.Id)
            .ToHashSet(),
            ( await invites.AllSentByUserAsync(IdentityContext.Id, cancellationToken) )
            .Select(fi => fi.InviteeId)
            .ToHashSet() );

        var userGroupsMemberIds = await ( await members.GroupsIdsByUser(IdentityContext.Id, cancellationToken) )
            .Select(groupId => members.UserIdsByGroup(groupId, cancellationToken));

        // Get members from user's chat groups they are member of:
        var groupsUsers = await users.GetByIds(
            userGroupsMemberIds
                .SelectMany(_ => _)
                .Distinct().Take(10), cancellationToken);

        // Filter out friends / self / exclude those whom the user has sent invitations to:
        return groupsUsers?.Where(Filter).ToList() ?? [];

        bool Filter(Domain.Entities.User user) => IsNotFriendOrMe(user, friendIds) && !userInviteeIds.Contains(user.Id);
    }

    private bool IsNotFriendOrMe(
        Domain.Entities.User user,
        IReadOnlySet<Guid> friendIds)
        => !friendIds.Contains(user.Id) && user.Id != IdentityContext.Id;
}
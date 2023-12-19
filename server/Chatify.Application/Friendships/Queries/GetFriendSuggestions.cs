using Chatify.Application.Common;
using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using Chatify.Shared.Infrastructure.Common.Extensions;

namespace Chatify.Application.Friendships.Queries;

[Cached(queryCacheKeyPrefix: "friend-suggestions", 60)]
public record GetFriendSuggestions() : IQuery<List<ChatGroupMember>>;

internal sealed class GetFriendSuggestionsHandler(
    IIdentityContext identityContext,
    IFriendshipsRepository friendships,
    IChatGroupMemberRepository members)
    :
        BaseQueryHandler<GetFriendSuggestions, List<ChatGroupMember>>(identityContext)
{
    public override async Task<List<ChatGroupMember>> HandleAsync(
        GetFriendSuggestions command,
        CancellationToken cancellationToken = default)
    {
        // Get user's friends:
        var friendIds = ( await friendships.AllForUser(_identityContext.Id, cancellationToken) )
            .Select(u => u.Id)
            .ToHashSet();

        // Get members from user's chat groups they are member of:
        var groupIds = await members
            .GroupsIdsByUser(_identityContext.Id, cancellationToken);

        var groupMembers = await groupIds
            .Take(5)
            .Select(groupId => members.ByGroup(groupId, cancellationToken))
            .ToList();

        // Filter out friends / self:
        return groupMembers
            .SelectMany(_ => _)
            .DistinctBy(m => m.UserId)
            .Where(m => IsNotFriendOrMe(m, friendIds))
            .ToList();
    }

    private bool IsNotFriendOrMe(ChatGroupMember member,
        HashSet<Guid> friendIds)
        => !friendIds.Contains(member.UserId) && member.UserId != _identityContext.Id;
}
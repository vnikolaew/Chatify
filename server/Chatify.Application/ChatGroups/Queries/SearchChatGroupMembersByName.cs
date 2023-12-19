using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.Common;
using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Application.Common.Behaviours.Timing;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.ChatGroups.Queries;

using SearchChatGroupMembersByNameResult = OneOf<UserIsNotMemberError, List<Domain.Entities.User>>;

[Cached("members", 60)]
public record SearchChatGroupMembersByName(
    [Required] [property: CacheKey] Guid GroupId,
    [property: CacheKey] string? SearchQuery
) : IQuery<SearchChatGroupMembersByNameResult>;

[Timed]
internal sealed class SearchChatGroupMembersByNameHandler(
    IChatGroupMemberRepository members,
    IIdentityContext identityContext,
    IUserRepository users)
    : BaseQueryHandler<SearchChatGroupMembersByName, SearchChatGroupMembersByNameResult>(identityContext)
{
    public override async Task<SearchChatGroupMembersByNameResult> HandleAsync(
        SearchChatGroupMembersByName query,
        CancellationToken cancellationToken = default)
    {
        var isMember = await members
            .Exists(query.GroupId, identityContext.Id, cancellationToken);
        if ( !isMember ) return new UserIsNotMemberError(identityContext.Id, query.GroupId);

        var groupMembers = await members.ByGroup(query.GroupId, cancellationToken);
        if ( groupMembers is null ) return default;

        // Execute an in-memory search:
        var userIds = groupMembers
            .Where(m => m.Username.Contains(
                query.SearchQuery ?? string.Empty,
                StringComparison.InvariantCultureIgnoreCase))
            .Select(m => m.UserId)
            .ToList();

        return await users.GetByIds(userIds, cancellationToken)
               ?? new List<Domain.Entities.User>();
    }
}
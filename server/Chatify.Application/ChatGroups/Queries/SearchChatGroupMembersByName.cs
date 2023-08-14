using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Application.Common.Behaviours.Timing;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.ChatGroups.Queries;

using SearchChatGroupMembersByNameResult = OneOf<Error, List<Domain.Entities.User>>;

[Cached("members", 60)]
public record SearchChatGroupMembersByName(
    [Required] [property: CacheKey] Guid GroupId,
    [property: CacheKey]string? SearchQuery
    ) : IQuery<SearchChatGroupMembersByNameResult>;

[Timed]
internal sealed class SearchChatGroupMembersByNameHandler
    : IQueryHandler<SearchChatGroupMembersByName, SearchChatGroupMembersByNameResult>
{
    private readonly IChatGroupMemberRepository _members;
    private readonly IUserRepository _users;
    private readonly IIdentityContext _identityContext;

    public SearchChatGroupMembersByNameHandler(
        IChatGroupMemberRepository members,
        IIdentityContext identityContext, IUserRepository users)
    {
        _members = members;
        _identityContext = identityContext;
        _users = users;
    }

    public async Task<SearchChatGroupMembersByNameResult> HandleAsync(
        SearchChatGroupMembersByName query,
        CancellationToken cancellationToken = default)
    {
        var isMember = await _members
            .Exists(query.GroupId, _identityContext.Id, cancellationToken);
        if ( !isMember ) return Error.New("");

        var members = await _members.ByGroup(query.GroupId, cancellationToken);
        if ( members is null ) return Error.New("");
        
        // Execute an in-memory search:
        var userIds = members
            .Where(m => m.Username.Contains(
                query.SearchQuery ?? string.Empty,
                StringComparison.InvariantCultureIgnoreCase))
            .Select(m => m.UserId)
            .ToList();
        
        return await _users.GetByIds(userIds, cancellationToken)
               ?? new List<Domain.Entities.User>();
    }
}
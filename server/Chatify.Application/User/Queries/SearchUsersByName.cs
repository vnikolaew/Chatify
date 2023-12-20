using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Application.User.Common;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Queries;
using OneOf;

namespace Chatify.Application.User.Queries;

using SearchUsersByNameResult = OneOf<UserNotFound, List<Domain.Entities.User>>;

[Cached("user-by-username", 30)]
public record SearchUsersByName(
    [Required] [property: CacheKey] string SearchQuery
) : IQuery<SearchUsersByNameResult>;

internal sealed class SearchUsersByNameHandler
    (IUserRepository users) : IQueryHandler<SearchUsersByName, SearchUsersByNameResult>
{
    public async Task<SearchUsersByNameResult> HandleAsync(SearchUsersByName command,
        CancellationToken cancellationToken = default)
    {
        // Figure out Full-Text search here:
        var searchUsers = await users.SearchByUsername(command.SearchQuery, cancellationToken);
        return searchUsers is null ? new UserNotFound() : searchUsers;
    }
}
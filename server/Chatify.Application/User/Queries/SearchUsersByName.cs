using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Application.User.Common;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Queries;
using OneOf;

namespace Chatify.Application.User.Queries;

using SearchUsersByNameResult = OneOf<UserNotFound, Domain.Entities.User>;

[Cached("user-by-username", 30)]
public record SearchUsersByName(
    [Required] [property: CacheKey] string SearchQuery
) : IQuery<SearchUsersByNameResult>;

internal sealed class SearchUsersByNameHandler
    : IQueryHandler<SearchUsersByName, SearchUsersByNameResult>
{
    private readonly IUserRepository _users;

    public SearchUsersByNameHandler(
        IUserRepository users)
        => _users = users;

    public async Task<SearchUsersByNameResult> HandleAsync(
        SearchUsersByName command,
        CancellationToken cancellationToken = default)
    {
        // Figure put Full-Text search here:
        var user = await _users.GetByUsername(command.SearchQuery, cancellationToken);
        if ( user is null ) return new UserNotFound();
        return user;
    }
}
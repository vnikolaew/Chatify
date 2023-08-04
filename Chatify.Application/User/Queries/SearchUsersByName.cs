using System.ComponentModel.DataAnnotations;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Queries;
using LanguageExt;
using LanguageExt.Common;

namespace Chatify.Application.User.Queries;
using SearchUsersByNameResult = Either<Error, Domain.Entities.User>;

public record SearchUsersByName(
    [Required] string NameSearchQuery
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
        var user = await _users.GetByUsername(
            command.NameSearchQuery, cancellationToken);
        
        return user;
    }
}

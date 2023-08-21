using System.ComponentModel.DataAnnotations;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.ChatGroups.Queries;

using SearchChatGroupsByNameResult = OneOf<Error, List<ChatGroup>>;

public record SearchChatGroupsByName(
    [Required] string NameSearchQuery
) : IQuery<SearchChatGroupsByNameResult>;

internal sealed class
    SearchChatGroupsByNameHandler : IQueryHandler<SearchChatGroupsByName, SearchChatGroupsByNameResult>
{
    private readonly IChatGroupMemberRepository _members;
    private readonly IChatGroupRepository _groups;
    private readonly IIdentityContext _identityContext;

    public SearchChatGroupsByNameHandler(
        IChatGroupMemberRepository members,
        IIdentityContext identityContext,
        IChatGroupRepository groups)
    {
        _members = members;
        _identityContext = identityContext;
        _groups = groups;
    }

    public async Task<SearchChatGroupsByNameResult> HandleAsync(
        SearchChatGroupsByName query,
        CancellationToken cancellationToken = default)
    {
        var groupIds = ( await _members
                .GroupsIdsByUser(_identityContext.Id, cancellationToken) )
            .ToHashSet();
        
        // Do an in-memory search (at least for now) as RedisSearch supports FT of full words only:
        return ( await _groups.GetByIds(groupIds, cancellationToken) )
            .Where(g => g.Name.Contains(query.NameSearchQuery, StringComparison.InvariantCultureIgnoreCase))
            .ToList();
    }
}
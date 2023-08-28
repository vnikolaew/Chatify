using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Models;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.ChatGroups.Queries;

using SearchChatGroupsByNameResult = OneOf<BaseError, List<ChatGroup>>;

public record SearchChatGroupsByName(
    [Required] string NameSearchQuery
) : IQuery<SearchChatGroupsByNameResult>;

internal sealed class
    SearchChatGroupsByNameHandler(IChatGroupMemberRepository members,
        IIdentityContext identityContext,
        IChatGroupRepository groups)
    : IQueryHandler<SearchChatGroupsByName, SearchChatGroupsByNameResult>
{
    public async Task<SearchChatGroupsByNameResult> HandleAsync(
        SearchChatGroupsByName query,
        CancellationToken cancellationToken = default)
    {
        var groupIds = ( await members
                .GroupsIdsByUser(identityContext.Id, cancellationToken) )
            .ToHashSet();
        
        // Do an in-memory search (at least for now) as RedisSearch supports FT of full words only:
        return ( await groups.GetByIds(groupIds, cancellationToken) )
            .Where(g => g.Name.Contains(query.NameSearchQuery, StringComparison.InvariantCultureIgnoreCase))
            .ToList();
    }
}
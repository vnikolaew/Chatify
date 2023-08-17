using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using OneOf;

namespace Chatify.Application.ChatGroups.Queries;
using SearchChatGroupByIdResult = OneOf<ChatGroupNotFoundError, ChatGroup>;

public record SearchChatGroupById(
    [Required] Guid GroupId
    ):  IQuery<SearchChatGroupByIdResult>;

internal sealed class SearchChatGroupByIdHandler : IQueryHandler<SearchChatGroupById, SearchChatGroupByIdResult>
{
    private readonly IIdentityContext _identityContext;
    private readonly IChatGroupRepository _groups;

    public SearchChatGroupByIdHandler(IIdentityContext identityContext, IChatGroupRepository groups)
    {
        _identityContext = identityContext;
        _groups = groups;
    }

    public async Task<SearchChatGroupByIdResult> HandleAsync(
        SearchChatGroupById query,
        CancellationToken cancellationToken = default)
    {
        var group = await _groups.GetAsync(query.GroupId, cancellationToken);
        return (SearchChatGroupByIdResult?) group ?? new ChatGroupNotFoundError();
    }
}
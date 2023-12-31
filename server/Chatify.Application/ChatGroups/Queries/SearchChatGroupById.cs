﻿using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.Common;
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

internal sealed class SearchChatGroupByIdHandler(
    IIdentityContext identityContext,
    IChatGroupRepository groups)
    : BaseQueryHandler<SearchChatGroupById, SearchChatGroupByIdResult>(identityContext)
{
    public override async Task<SearchChatGroupByIdResult> HandleAsync(SearchChatGroupById query,
        CancellationToken cancellationToken = default)
    {
        var group = await groups.GetAsync(query.GroupId, cancellationToken);
        return (SearchChatGroupByIdResult?) group ?? new ChatGroupNotFoundError();
    }
}
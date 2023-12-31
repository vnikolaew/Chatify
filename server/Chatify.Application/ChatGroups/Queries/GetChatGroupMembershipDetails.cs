﻿using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.Common;
using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using OneOf;
using Error = LanguageExt.Common.Error;

namespace Chatify.Application.ChatGroups.Queries;

using GetChatGroupMembershipDetailsResult = OneOf<UserIsNotMemberError, Error, ChatGroupMember>;

[Cached("chat-group-membership", 60 * 10)]
public record GetChatGroupMembershipDetails(
    [Required] [property: CacheKey] Guid GroupId,
    [Required] [property: CacheKey] Guid UserId
) : IQuery<GetChatGroupMembershipDetailsResult>;

internal sealed class
    GetChatGroupMembershipDetailsHandler(
        IIdentityContext identityContext,
        IChatGroupMemberRepository members)
    : BaseQueryHandler<GetChatGroupMembershipDetails,
        GetChatGroupMembershipDetailsResult>(identityContext)
{
    public override async Task<GetChatGroupMembershipDetailsResult> HandleAsync(GetChatGroupMembershipDetails query,
        CancellationToken cancellationToken = default)
    {
        var isMember = await members.Exists(query.GroupId, identityContext.Id, cancellationToken);
        if ( !isMember ) return new UserIsNotMemberError(identityContext.Id, query.GroupId);

        var membership = await members
            .ByGroupAndUser(query.GroupId, query.UserId, cancellationToken);

        return membership is not null ? membership : Error.New("");
    }
}
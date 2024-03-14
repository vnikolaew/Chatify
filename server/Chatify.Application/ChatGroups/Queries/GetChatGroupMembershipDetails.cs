using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.ChatGroups.Contracts;
using Chatify.Application.Common;
using Chatify.Application.Common.Behaviours.Caching;
using Chatify.Domain.Entities;
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

internal sealed class GetChatGroupMembershipDetailsHandler(
    IChatGroupsService chatGroupsService,
    IIdentityContext identityContext)
    : BaseQueryHandler<GetChatGroupMembershipDetails,
        GetChatGroupMembershipDetailsResult>(identityContext)
{
    public override async Task<GetChatGroupMembershipDetailsResult> HandleAsync(GetChatGroupMembershipDetails query,
        CancellationToken cancellationToken = default)
    {
        var response = await chatGroupsService.GetChatGroupMembershipDetailsAsync(
            new GetChatGroupMembershipDetailsRequest(query.GroupId, query.UserId), cancellationToken);
        return response.Value is Error error ? error : response.AsT1;
    }
}
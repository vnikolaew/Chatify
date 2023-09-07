using Chatify.Application.ChatGroups.Queries;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Extensions;
using FastEndpoints;
using GetChatGroupMembershipDetailsResult =
    OneOf.OneOf<Chatify.Application.ChatGroups.Commands.UserIsNotMemberError, LanguageExt.Common.Error,
        Chatify.Domain.Entities.ChatGroupMember>;

namespace Chatify.Web.FastEndpoints_Features.ChatGroups;

[HttpGet("members/{groupId:guid}/{memberId:guid}")]
public sealed class MembershipDetailsEndpoint
    : BaseChatGroupsEndpoint<GetChatGroupMembershipDetails, IResult>
{
    public override Task<IResult> HandleAsync(GetChatGroupMembershipDetails req,
        CancellationToken ct)
    {
        var groupId = Route<Guid>("groupId");
        var memberId = Route<Guid>("memberId");

        return QueryAsync<GetChatGroupMembershipDetails, GetChatGroupMembershipDetailsResult>(
                new GetChatGroupMembershipDetails(groupId, memberId),
                ct)
            .MatchAsync(
                _ => _.ToBadRequestResult(),
                _ => TypedResults.NotFound(),
                Ok);
    }
}
using Chatify.Application.ChatGroups.Commands;
using Chatify.Web.Extensions;
using FastEndpoints;
using RemoveChatGroupMemberResult =
    OneOf.OneOf<Chatify.Application.ChatGroups.Commands.ChatGroupNotFoundError,
        Chatify.Application.ChatGroups.Commands.UserIsNotMemberError,
        Chatify.Application.ChatGroups.Commands.UserIsNotGroupAdminError, LanguageExt.Unit>;

namespace Chatify.Web.FastEndpoints_Features.ChatGroups;

[HttpDelete("members")]
public sealed class RemoveMemberEndpoint : BaseChatGroupsEndpoint<RemoveChatGroupMember, IResult>
{
    public override async Task<IResult> HandleAsync(RemoveChatGroupMember req,
        CancellationToken ct)
    {
        var result = await SendAsync<RemoveChatGroupMember, RemoveChatGroupMemberResult>(
            req,
            ct);
        return result.Match(
            _ => TypedResults.NotFound(),
            _ => _.ToBadRequestResult(),
            _ => _.ToBadRequestResult(),
            NoContent);
    }
}
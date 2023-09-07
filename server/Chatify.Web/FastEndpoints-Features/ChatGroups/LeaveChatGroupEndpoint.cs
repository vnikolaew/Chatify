using Chatify.Application.ChatGroups.Commands;
using Chatify.Web.Extensions;
using FastEndpoints;
using LeaveChatGroupResult =
    OneOf.OneOf<Chatify.Application.ChatGroups.Commands.ChatGroupNotFoundError,
        Chatify.Application.ChatGroups.Commands.UserIsNotMemberError, LanguageExt.Unit>;

namespace Chatify.Web.FastEndpoints_Features.ChatGroups;

[HttpPost("leave")]
public sealed class LeaveChatGroupEndpoint : BaseChatGroupsEndpoint<LeaveChatGroup, IResult>
{
    public override async Task<IResult> HandleAsync(
        LeaveChatGroup req,
        CancellationToken ct)
    {
        var result = await SendAsync<LeaveChatGroup, LeaveChatGroupResult>(
            req,
            ct);

        return result.Match(
            _ => TypedResults.BadRequest(),
            _ => _.ToBadRequestResult(),
            Accepted);
    }
}
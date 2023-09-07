using Chatify.Application.ChatGroups.Commands;
using Chatify.Web.Extensions;
using FastEndpoints;
using AddChatGroupAdminResult =
    OneOf.OneOf<Chatify.Application.ChatGroups.Commands.ChatGroupNotFoundError,
        Chatify.Application.ChatGroups.Commands.UserIsNotMemberError,
        Chatify.Application.ChatGroups.Commands.UserIsNotGroupAdminError, LanguageExt.Unit>;

namespace Chatify.Web.FastEndpoints_Features.ChatGroups;

[HttpPost("admins")]
public sealed class AddAdminEndpoint : BaseChatGroupsEndpoint<AddChatGroupAdmin, IResult>
{
    public override async Task<IResult> HandleAsync(AddChatGroupAdmin req,
        CancellationToken ct)
    {
        var result = await SendAsync<AddChatGroupAdmin, AddChatGroupAdminResult>(
            req,
            ct);
        return result
            .Match(
                _ => TypedResults.NotFound(),
                _ => _.ToBadRequestResult(),
                _ => _.ToBadRequestResult(),
                _ => Accepted());
    }
}
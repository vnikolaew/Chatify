using Chatify.Application.ChatGroups.Commands;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Extensions;
using Chatify.Web.Features.ChatGroups.Models;
using FastEndpoints;
using EditChatGroupDetailsResult =
    OneOf.OneOf<Chatify.Application.ChatGroups.Commands.ChatGroupNotFoundError,
        Chatify.Application.User.Commands.FileUploadError, Chatify.Application.ChatGroups.Commands.UserIsNotMemberError,
        Chatify.Application.ChatGroups.Commands.UserIsNotGroupAdminError, LanguageExt.Unit>;
using Guid = System.Guid;

namespace Chatify.Web.FastEndpoints_Features.ChatGroups;

[HttpPatch("{groupId:guid}")]
public sealed class EditEndpoint : BaseChatGroupsEndpoint<Models.EditChatGroupDetailsRequest, IResult>
{
    public override async Task<IResult> HandleAsync(Models.EditChatGroupDetailsRequest req,
        CancellationToken ct)
    {
        var groupId = Route<Guid>("groupId");
        return await SendAsync<EditChatGroupDetails, EditChatGroupDetailsResult>(
                ( req with { ChatGroupId = groupId } ).ToCommand(),
                ct)
            .MatchAsync(
                _ => ( IResult )TypedResults.NotFound(),
                err => err.ToBadRequestResult(),
                err => err.ToBadRequestResult(),
                _ => _.ToBadRequestResult(),
                _ => TypedResults.Accepted(string.Empty));
    }
}
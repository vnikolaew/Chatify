using Chatify.Application.ChatGroups.Commands;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Common;
using Chatify.Web.Extensions;
using Chatify.Web.Features.ChatGroups.Models;
using FastEndpoints;
using CreateChatGroupResult = OneOf.OneOf<Chatify.Application.User.Commands.FileUploadError, LanguageExt.Common.Error, System.Guid>;

namespace Chatify.Web.FastEndpoints_Features.ChatGroups;

[HttpPost]
public sealed class CreateChatGroupEndpoint : BaseChatGroupsEndpoint<Models.CreateChatGroupRequest, IResult>
{
    public override Task<IResult> HandleAsync(
        Models.CreateChatGroupRequest req,
        CancellationToken ct)
        => SendAsync<CreateChatGroup, CreateChatGroupResult>(req.ToCommand(), ct)
            .MatchAsync(
                err => err.ToBadRequestResult(),
                err => err.ToBadRequestResult(),
                id => TypedResults.CreatedAtRoute(
                    ApiResponse<object>.Success(new { groupId = id },
                        "Chat group successfully created."),
                    "ChatGroups/Details",
                    new { groupId = id }
                ));
}
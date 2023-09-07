using Chatify.Application.ChatGroups.Commands;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Extensions;
using FastEndpoints;
using AddChatGroupMemberResult =
    OneOf.OneOf<Chatify.Application.User.Common.UserNotFound,
        Chatify.Application.ChatGroups.Commands.UserIsNotGroupAdminError,
        Chatify.Application.ChatGroups.Commands.ChatGroupNotFoundError,
        Chatify.Application.ChatGroups.Commands.UserIsAlreadyGroupMemberError, System.Guid>;

namespace Chatify.Web.FastEndpoints_Features.ChatGroups;

[HttpPost("members")]
public sealed class AddMemberEndpoint : BaseChatGroupsEndpoint<AddChatGroupMember, IResult>
{
    public override Task<IResult> HandleAsync(AddChatGroupMember req,
        CancellationToken ct)
        => SendAsync<AddChatGroupMember, AddChatGroupMemberResult>(
                req,
                ct)
            .MatchAsync(
                _ => ( IResult )TypedResults.NotFound(),
                _ => _.ToBadRequestResult(),
                _ => TypedResults.NotFound(),
                _ => _.ToBadRequestResult(),
                id => TypedResults.Accepted(string.Empty, id));
}
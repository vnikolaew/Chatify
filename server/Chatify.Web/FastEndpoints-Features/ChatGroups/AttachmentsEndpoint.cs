using Chatify.Application.ChatGroups.Queries;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Chatify.Web.Extensions;
using Chatify.Web.Features.ChatGroups.Models;
using GetChatGroupSharedAttachmentsResult =
    OneOf.OneOf<Chatify.Application.ChatGroups.Commands.UserIsNotMemberError, Chatify.Shared.Abstractions.Queries.
        CursorPaged<Chatify.Domain.Entities.ChatGroupAttachment>>;

namespace Chatify.Web.FastEndpoints_Features.ChatGroups;

[FastEndpoints.HttpGet("{groupId:guid}/attachments")]
public sealed class AttachmentsEndpoint : BaseChatGroupsEndpoint<Models.GetChatGroupSharedAttachmentsRequest, IResult>
{
    public override async Task<IResult> HandleAsync(
        Models.GetChatGroupSharedAttachmentsRequest req,
        CancellationToken ct)
    {
        var groupId = Route<Guid>("groupId");

        return await QueryAsync<GetChatGroupSharedAttachments, GetChatGroupSharedAttachmentsResult>(
                ( req with { GroupId = groupId } ).ToCommand(), ct)
            .MatchAsync(
                _ => _.ToBadRequestResult(),
                Ok);
    }
}
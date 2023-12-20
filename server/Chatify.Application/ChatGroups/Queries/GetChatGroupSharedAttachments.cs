using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.Common;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Contexts;
using Chatify.Shared.Abstractions.Queries;
using OneOf;

namespace Chatify.Application.ChatGroups.Queries;

using GetChatGroupSharedAttachmentsResult = OneOf<UserIsNotMemberError, CursorPaged<ChatGroupAttachment>>;

public record GetChatGroupSharedAttachments(
    [Required] Guid GroupId,
    [Required] int PageSize,
    [Required] string PagingCursor
) : IQuery<GetChatGroupSharedAttachmentsResult>;

internal sealed class GetChatGroupSharedAttachmentsHandler(
    IIdentityContext identityContext,
    IChatGroupMemberRepository members,
    IChatGroupAttachmentRepository attachments)
    : BaseQueryHandler<GetChatGroupSharedAttachments, GetChatGroupSharedAttachmentsResult>(identityContext)
{
    public override async Task<GetChatGroupSharedAttachmentsResult> HandleAsync(GetChatGroupSharedAttachments query,
        CancellationToken cancellationToken = default)
    {
        var isGroupMember = await members.Exists(
            query.GroupId,
            identityContext.Id, cancellationToken);
        if ( !isGroupMember ) return new UserIsNotMemberError(identityContext.Id, query.GroupId);

        return await attachments.GetPaginatedAttachmentsByGroupAsync(
            query.GroupId,
            query.PageSize,
            query.PagingCursor, cancellationToken);
    }
}
using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Commands;
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

internal sealed class GetChatGroupSharedAttachmentsHandler
    : IQueryHandler<GetChatGroupSharedAttachments, GetChatGroupSharedAttachmentsResult>
{
    private readonly IIdentityContext _identityContext;
    private readonly IChatGroupAttachmentRepository _attachments;
    private readonly IChatGroupMemberRepository _members;

    public GetChatGroupSharedAttachmentsHandler(
        IIdentityContext identityContext,
        IChatGroupMemberRepository members,
        IChatGroupAttachmentRepository attachments)
    {
        _identityContext = identityContext;
        _members = members;
        _attachments = attachments;
    }

    public async Task<GetChatGroupSharedAttachmentsResult> HandleAsync(
        GetChatGroupSharedAttachments query,
        CancellationToken cancellationToken = default)
    {
        var isGroupMember = await _members.Exists(
            query.GroupId,
            _identityContext.Id, cancellationToken);
        if ( !isGroupMember ) return new UserIsNotMemberError(_identityContext.Id, query.GroupId);

        var attachments = await _attachments.GetPaginatedAttachmentsByGroupAsync(
            query.GroupId,
            query.PageSize,
            query.PagingCursor, cancellationToken);

        return attachments;
    }
}
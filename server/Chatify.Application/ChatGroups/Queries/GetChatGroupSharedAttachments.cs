using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Commands;
using Chatify.Application.ChatGroups.Contracts;
using Chatify.Application.Common;
using Chatify.Domain.Entities;
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
    IChatGroupsService chatGroupsService,
    IIdentityContext identityContext
)
    : BaseQueryHandler<GetChatGroupSharedAttachments, GetChatGroupSharedAttachmentsResult>(identityContext)
{
    public override async Task<GetChatGroupSharedAttachmentsResult> HandleAsync(GetChatGroupSharedAttachments query,
        CancellationToken cancellationToken = default)
        => await chatGroupsService.GetChatGroupSharedAttachments(query, cancellationToken);
}
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Shared.Abstractions.Queries;

namespace Chatify.Domain.Repositories;

public interface IChatGroupAttachmentRepository
    : IDomainRepository<ChatGroupAttachment, Guid>
{
    Task<IEnumerable<ChatGroupAttachment>> SaveManyAsync(
        IEnumerable<ChatGroupAttachment> entity,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteByIdAsync(
        Guid attachmentId, CancellationToken cancellationToken = default);

    Task<CursorPaged<ChatGroupAttachment>> GetPaginatedAttachmentsByGroupAsync(
        Guid groupId,
        int pageSize,
        string pagingCursor,
        CancellationToken cancellationToken = default);
}
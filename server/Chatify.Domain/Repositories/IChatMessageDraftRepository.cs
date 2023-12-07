using Chatify.Domain.Common;
using Chatify.Domain.Entities;

namespace Chatify.Domain.Repositories;

public interface IChatMessageDraftRepository : IDomainRepository<ChatMessageDraft, Guid>
{
    Task<List<ChatMessageDraft>> AllForUser(Guid userId,
        CancellationToken cancellationToken = default);
    
    Task<ChatMessageDraft?> ForUserAndGroup(
        Guid userId,
        Guid groupId,
        CancellationToken cancellationToken = default);
}
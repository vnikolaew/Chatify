using Chatify.Domain.Common;

namespace Chatify.Domain.Entities;

public interface IChatMessageReactionRepository : IDomainRepository<ChatMessageReaction, Guid>
{
    Task<bool> Exists(Guid messageId, Guid userId, CancellationToken cancellationToken = default);
    
    Task<ChatMessageReaction?> ByMessageAndUser(Guid messageId, Guid userId, CancellationToken cancellationToken = default);
}
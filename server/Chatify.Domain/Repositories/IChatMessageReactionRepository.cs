using Chatify.Domain.Common;
using Chatify.Domain.Entities;

namespace Chatify.Domain.Repositories;

public interface IChatMessageReactionRepository : IDomainRepository<ChatMessageReaction, Guid>
{
    Task<bool> Exists(Guid messageId, Guid userId, CancellationToken cancellationToken = default);
    
    Task<ChatMessageReaction?> ByMessageAndUser(Guid messageId, Guid userId, CancellationToken cancellationToken = default);
}
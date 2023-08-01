using Chatify.Domain.Common;

namespace Chatify.Domain.Entities;

public interface IChatMessageReplyRepository : IDomainRepository<ChatMessageReply, Guid>
{
    Task<bool> DeleteAllForMessage(Guid messageId, CancellationToken cancellationToken = default);
}
using Chatify.Domain.Common;
using Chatify.Domain.Entities;

namespace Chatify.Domain.Repositories;

public interface IChatMessageReplierInfosRepository
    : IDomainRepository<MessageRepliersInfo, Guid>
{
    Task<bool> DeleteAllForMessage(Guid messageId, CancellationToken cancellationToken = default);
}
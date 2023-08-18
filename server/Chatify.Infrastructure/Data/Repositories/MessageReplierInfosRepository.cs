using AutoMapper;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Data.Models;
using Chatify.Infrastructure.Data.Services;
using Humanizer;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public class MessageReplierInfosRepository(IMapper mapper, Mapper dbMapper, IEntityChangeTracker changeTracker)
    : BaseCassandraRepository<MessageRepliersInfo, ChatMessageRepliesSummary, Guid>(mapper, dbMapper, changeTracker,
            nameof(MessageRepliersInfo.ChatGroupId).Underscore()),
        IChatMessageReplierInfosRepository
{
    public async Task<bool> DeleteAllForMessage(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await DbMapper.DeleteAsync<ChatMessageRepliesSummary>(
                "WHERE message_id = ?;", messageId);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
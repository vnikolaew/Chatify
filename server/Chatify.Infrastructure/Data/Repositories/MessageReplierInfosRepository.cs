using AutoMapper;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Data.Models;
using Humanizer;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public class MessageReplierInfosRepository
    : BaseCassandraRepository<MessageRepliersInfo, ChatMessageRepliesSummary, Guid>,
        IChatMessageReplierInfosRepository
{
    public MessageReplierInfosRepository(
        IMapper mapper, Mapper dbMapper)
        : base(mapper, dbMapper, nameof(MessageRepliersInfo.ChatGroupId).Underscore())
    {
    }

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
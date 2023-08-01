using AutoMapper;
using Chatify.Domain.Entities;
using Humanizer;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class ChatMessageReplyRepository
    : BaseCassandraRepository<ChatMessageReply, Models.ChatMessageReply, Guid>,
        IChatMessageReplyRepository
{
    public ChatMessageReplyRepository(
        IMapper mapper,
        Mapper dbMapper) : base(mapper, dbMapper, nameof(ChatMessageReply.Id).Underscore())
    {
    }

    public async Task<bool> DeleteAllForMessage(Guid messageId, CancellationToken cancellationToken = default)
    {
        try
        {
            await DbMapper.DeleteAsync<Models.ChatMessageReply>("WHERE reply_to_id = ?", messageId);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
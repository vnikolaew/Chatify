using AutoMapper;
using Chatify.Domain.Entities;
using Chatify.Shared.Abstractions.Queries;
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

    public async Task<CursorPaged<ChatMessageReply>> GetPaginatedByMessageAsync(
        Guid messageId,
        int pageSize,
        string pagingCursor,
        CancellationToken cancellationToken)
    {
        var messagesPage = await DbMapper.FetchPageAsync<Models.ChatMessageReply>(
            pageSize, CassandraPagingHelper.ToPagingState(pagingCursor), "WHERE reply_to_id = ?",
            new object[] { messageId });

        return new CursorPaged<ChatMessageReply>(
            Mapper.Map<List<ChatMessageReply>>(messagesPage.ToList()),
            CassandraPagingHelper.ToPagingCursor(messagesPage.PagingState));
    }
}
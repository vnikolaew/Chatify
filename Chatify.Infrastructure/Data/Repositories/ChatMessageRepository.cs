using AutoMapper;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Shared.Abstractions.Queries;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class ChatMessageRepository :
    BaseCassandraRepository<ChatMessage, Models.ChatMessage, Guid>,
    IChatMessageRepository
{
    public ChatMessageRepository(
        IMapper mapper, Mapper dbMapper)
        : base(mapper, dbMapper)
    {
    }


    public async Task<CursorPaged<ChatMessage>> GetPaginatedByGroupAsync(
        Guid groupId,
        int pageSize,
        string pagingCursor,
        CancellationToken cancellationToken = default)
    {
        var messagesPage = await DbMapper.FetchPageAsync<Models.ChatMessage>(
            pageSize, CassandraPagingHelper.ToPagingState(pagingCursor), "WHERE chat_group_id = ?",
            new object[] { groupId });

        return new CursorPaged<ChatMessage>(
            Mapper.Map<List<ChatMessage>>(messagesPage.ToList()),
            CassandraPagingHelper.ToPagingCursor(messagesPage.PagingState)
        );
    }
}

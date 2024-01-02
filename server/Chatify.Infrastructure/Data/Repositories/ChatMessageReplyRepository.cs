using AutoMapper;
using Chatify.Application.Common.Contracts;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Mappings;
using Chatify.Infrastructure.Data.Services;
using Chatify.Shared.Abstractions.Queries;
using Humanizer;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class ChatMessageReplyRepository(IMapper mapper,
        IEntityChangeTracker changeTracker,
        Mapper dbMapper,
        IPagingCursorHelper pagingCursorHelper)
    : BaseCassandraRepository<ChatMessageReply, Models.ChatMessageReply, Guid>(mapper, dbMapper, changeTracker,
            nameof(ChatMessageReply.Id).Underscore()),
        IChatMessageReplyRepository
{
    public async Task<bool> DeleteAllForMessage(Guid messageId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await DbMapper.DeleteAsync<Models.ChatMessageReply>(
                "WHERE reply_to_id = ?;", messageId);
            return true;
        }
        catch ( Exception )
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
            pageSize, pagingCursorHelper.ToPagingState(pagingCursor), "WHERE reply_to_id = ?;",
            [messageId]);
        var total = await DbMapper.FirstOrDefaultAsync<long>(
            "SELECT COUNT(*) FROM chat_message_replies WHERE reply_to_id = ?;",
            messageId);

        return new CursorPaged<ChatMessageReply>(
            messagesPage
                .To<ChatMessageReply>(Mapper)
                .ToList(),
            pagingCursorHelper.ToPagingCursor(messagesPage.PagingState)!,
            messagesPage.Count, total,
            messagesPage.PagingState is not null);
    }
}
using AutoMapper;
using Chatify.Application.Common.Contracts;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Mappings;
using Chatify.Shared.Abstractions.Queries;
using Humanizer;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class ChatMessageReplyRepository
    : BaseCassandraRepository<ChatMessageReply, Models.ChatMessageReply, Guid>,
        IChatMessageReplyRepository
{
    private readonly IPagingCursorHelper _pagingCursorHelper;

    public ChatMessageReplyRepository(
        IMapper mapper,
        Mapper dbMapper, IPagingCursorHelper pagingCursorHelper) : base(mapper, dbMapper,
        nameof(ChatMessageReply.Id).Underscore())
    {
        _pagingCursorHelper = pagingCursorHelper;
    }

    public async Task<bool> DeleteAllForMessage(Guid messageId, CancellationToken cancellationToken = default)
    {
        try
        {
            await DbMapper.DeleteAsync<Models.ChatMessageReply>("WHERE reply_to_id = ?", messageId);
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
            pageSize, _pagingCursorHelper.ToPagingState(pagingCursor), "WHERE reply_to_id = ?",
            new object[] { messageId });

        return new CursorPaged<ChatMessageReply>(
            messagesPage
                .AsQueryable()
                .To<ChatMessageReply>(Mapper)
                .ToList(),
            _pagingCursorHelper.ToPagingCursor(messagesPage.PagingState));
    }
}
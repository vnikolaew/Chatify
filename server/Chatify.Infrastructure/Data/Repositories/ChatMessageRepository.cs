using System.Reflection;
using Cassandra.Mapping;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Data.Models;
using Chatify.Shared.Abstractions.Queries;
using ChatMessage = Chatify.Domain.Entities.ChatMessage;
using IMapper = AutoMapper.IMapper;
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

    public async Task<CursorPaged<MessageRepliersInfo>> GetPaginatedReplierInfosByGroupAsync(
        Guid groupId, int pageSize, string pagingCursor,
        CancellationToken cancellationToken = default)
    {
        var messagesPage = await DbMapper.FetchPageAsync<ChatMessageRepliesSummary>(
            pageSize, CassandraPagingHelper.ToPagingState(pagingCursor), "WHERE chat_group_id = ?",
            new object[] { groupId });

        return new CursorPaged<MessageRepliersInfo>(
            Mapper.Map<List<MessageRepliersInfo>>(messagesPage.ToList()),
            CassandraPagingHelper.ToPagingCursor(messagesPage.PagingState));
    }

    public async Task<IDictionary<Guid, ChatMessage>> GetLatestForGroups(
        IEnumerable<Guid> groupIds,
        CancellationToken cancellationToken = default)
    {
        var paramPlaceholders = string.Join(", ", groupIds.Select(_ => "?"));
        var cql = new Cql($" WHERE chat_group_id IN ({paramPlaceholders})");

        cql.GetType()
            .GetProperty(nameof(Cql.Arguments),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?
            .SetValue(cql, groupIds);

        var messages = await DbMapper
            .FetchAsync<Models.ChatMessage>(cql);

        return Mapper.Map<List<ChatMessage>>(
                messages ?? new List<Models.ChatMessage>())
            .ToDictionary(m => m.ChatGroupId);
    }
}
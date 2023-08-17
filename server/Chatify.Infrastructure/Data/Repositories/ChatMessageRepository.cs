using System.Reflection;
using Cassandra.Mapping;
using Chatify.Application.Common.Contracts;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Mappings;
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
    private readonly IPagingCursorHelper _pagingCursorHelper;

    public ChatMessageRepository(
        IMapper mapper, Mapper dbMapper, IPagingCursorHelper pagingCursorHelper)
        : base(mapper, dbMapper)
        => _pagingCursorHelper = pagingCursorHelper;

    public async Task<CursorPaged<ChatMessage>> GetPaginatedByGroupAsync(
        Guid groupId,
        int pageSize,
        string pagingCursor,
        CancellationToken cancellationToken = default)
    {
        var messagesPage = await DbMapper.FetchPageAsync<Models.ChatMessage>(
            pageSize, _pagingCursorHelper.ToPagingState(pagingCursor), "WHERE chat_group_id = ?",
            new object[] { groupId });

        return new CursorPaged<ChatMessage>(
            messagesPage.To<ChatMessage>(Mapper).ToList(),
            _pagingCursorHelper.ToPagingCursor(messagesPage.PagingState)
        );
    }

    public async Task<CursorPaged<MessageRepliersInfo>> GetPaginatedReplierInfosByGroupAsync(
        Guid groupId, int pageSize, string pagingCursor,
        CancellationToken cancellationToken = default)
    {
        var replierInfoes = await DbMapper.FetchPageAsync<ChatMessageRepliesSummary>(
            pageSize, _pagingCursorHelper.ToPagingState(pagingCursor), "WHERE chat_group_id = ?;",
            new object[] { groupId });

        return new CursorPaged<MessageRepliersInfo>(
            replierInfoes
                .To<MessageRepliersInfo>(Mapper)
                .ToList(),
            _pagingCursorHelper.ToPagingCursor(replierInfoes.PagingState));
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
            .SetValue(cql, groupIds.ToArray());

        var messages = await DbMapper
            .FetchAsync<Models.ChatMessage>(cql);

        return messages
            .To<ChatMessage>(Mapper)
            .ToDictionary(m => m.ChatGroupId);
    }

    public async Task<List<ChatMessage>?> GetByIds(
        IEnumerable<Guid> messageIds,
        CancellationToken cancellationToken = default)
    {
        var paramPlaceholders = string.Join(", ", messageIds.Select(_ => "?"));
        var cql = new Cql($" WHERE id IN ({paramPlaceholders}) ALLOW FILTERING;");

        cql.GetType()
            .GetProperty(nameof(Cql.Arguments),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?
            .SetValue(cql, messageIds.ToArray());

        var messages = await DbMapper
            .FetchAsync<Models.ChatMessage>(cql);

        return messages.To<ChatMessage>(Mapper).ToList();
    }
}
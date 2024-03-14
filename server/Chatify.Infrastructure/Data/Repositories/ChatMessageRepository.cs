using Cassandra.Mapping;
using Chatify.Application.Common.Contracts;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Mappings;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Chatify.Infrastructure.Data.Services;
using Chatify.Shared.Abstractions.Queries;
using Humanizer;
using ChatMessage = Chatify.Domain.Entities.ChatMessage;
using IMapper = AutoMapper.IMapper;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class ChatMessageRepository(
    IMapper mapper,
    Mapper dbMapper,
    IEntityChangeTracker changeTracker,
    IPagingCursorHelper pagingCursorHelper)
    :
        BaseCassandraRepository<ChatMessage, Models.ChatMessage, Guid>(mapper, dbMapper, changeTracker,
            nameof(ChatMessage.Id).Underscore()),
        IChatMessageRepository
{
    private Task<long> GetTotalMessagesCount(Guid groupId)
        => DbMapper.FirstOrDefaultAsync<long>(
            "SELECT COUNT(*) FROM chat_messages WHERE chat_group_id = ?", groupId);

    private Task<long> GetTotalMessagesRepliersCount(Guid groupId)
        => DbMapper.FirstOrDefaultAsync<long>(
            "SELECT COUNT(*) FROM chat_message_replies_summaries WHERE chat_group_id = ?", groupId);

    public async Task<CursorPaged<ChatMessage>> GetPaginatedByGroupAsync(
        Guid groupId,
        int pageSize,
        string pagingCursor,
        CancellationToken cancellationToken = default)
    {
        var messagesPage = await DbMapper.FetchPageAsync<Models.ChatMessage>(
            pageSize, pagingCursorHelper.ToPagingState(pagingCursor), "WHERE chat_group_id = ?",
            [groupId]);

        var total = await GetTotalMessagesCount(groupId);
        return new CursorPaged<ChatMessage>(
            messagesPage.To<ChatMessage>(Mapper).ToList(),
            pagingCursorHelper.ToPagingCursor(messagesPage.PagingState)!,
            messagesPage.Count,
            total,
            messagesPage.PagingState is not null
        );
    }

    public async Task<CursorPaged<MessageRepliersInfo>> GetPaginatedReplierInfosByGroupAsync(
        Guid groupId,
        int pageSize,
        string pagingCursor,
        CancellationToken cancellationToken = default)
    {
        var replierInfoes = await DbMapper.FetchPageAsync<ChatMessageRepliesSummary>(
            pageSize, pagingCursorHelper.ToPagingState(pagingCursor), "WHERE chat_group_id = ?;",
            [groupId]);

        var total = await GetTotalMessagesRepliersCount(groupId);
        return new CursorPaged<MessageRepliersInfo>(
            replierInfoes
                .Select(ri => ri.To<MessageRepliersInfo>(Mapper))
                .ToList(),
            pagingCursorHelper.ToPagingCursor(replierInfoes.PagingState)!,
            replierInfoes.Count,
            total,
            replierInfoes.PagingState is not null
        );
    }

    public async Task<IDictionary<Guid, ChatMessage?>> GetLatestForGroups(
        IEnumerable<Guid> groupIds,
        CancellationToken cancellationToken = default)
    {
        var paramPlaceholders = string.Join(", ", groupIds.Select(_ => "?"));
        var cql = new Cql($" WHERE chat_group_id IN ({paramPlaceholders}) PER PARTITION LIMIT 1;")
            .WithArguments(groupIds.Cast<object>().ToArray());

        var messages = await DbMapper.FetchListAsync<Models.ChatMessage>(cql);

        return groupIds.ToDictionary(id => id, id =>
                messages
                    .FirstOrDefault(m => m.ChatGroupId == id)?
                    .To<ChatMessage>(Mapper));
    }

    public async Task<List<ChatMessage>?> GetByIds(
        IEnumerable<Guid> messageIds,
        CancellationToken cancellationToken = default)
    {
        var paramPlaceholders = string.Join(", ", messageIds.Select(_ => "?"));
        var cql = new Cql($"SELECT * FROM chat_messages_by_id WHERE id IN ({paramPlaceholders}) ALLOW FILTERING;")
            .WithArguments(messageIds.Cast<object>().ToArray());

        var messages = await DbMapper
            .FetchListAsync<Models.ChatMessage>(cql);

        return messages
            .To<ChatMessage>(Mapper)
            .ToList();
    }
}
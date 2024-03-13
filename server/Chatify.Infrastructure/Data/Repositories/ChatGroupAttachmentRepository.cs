using AutoMapper;
using Chatify.Application.Common.Contracts;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Mappings;
using Chatify.Infrastructure.Data.Services;
using Chatify.Shared.Abstractions.Queries;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Humanizer;
using LanguageExt;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public class ChatGroupAttachmentRepository(IMapper mapper,
        IEntityChangeTracker changeTracker,
        Mapper dbMapper,
        IPagingCursorHelper pagingCursorHelper)
    : BaseCassandraRepository<ChatGroupAttachment, Models.ChatGroupAttachment, Guid>(mapper, dbMapper, changeTracker,
            nameof(Models.ChatGroupAttachment.ChatGroupId).Underscore()),
        IChatGroupAttachmentRepository
{
    public async Task<IEnumerable<ChatGroupAttachment>> SaveManyAsync(
        IEnumerable<ChatGroupAttachment> attachments,
        CancellationToken cancellationToken = default)
    {
        foreach ( var chunk in attachments.Chunk(3) )
        {
            var saveTasks = chunk
                .Select(a =>
                    DbMapper.InsertAsync(a.To<Models.ChatGroupAttachment>(Mapper), insertNulls: true))
                .ToArray();

            await saveTasks;
        }

        return attachments;
    }

    public Task<bool> DeleteByIdAsync(
        Guid attachmentId,
        CancellationToken cancellationToken = default)
        => new TryAsync<bool>(
                async () =>
                {
                    var idColumn = nameof(Models.ChatGroupAttachment.AttachmentId).Underscore();
                    await DbMapper.DeleteAsync<Models.ChatGroupAttachment>(
                        $" WHERE {idColumn} = ? ALLOW FILTERING;",
                        attachmentId);
                    return true;
                })
            .Try()
            .Map(r => r.Match(_ => true, _ => false));

    public async Task<CursorPaged<ChatGroupAttachment>> GetPaginatedAttachmentsByGroupAsync(
        Guid groupId, int pageSize, string pagingCursor,
        CancellationToken cancellationToken = default)
    {
        var attachmentsPage = await DbMapper.FetchPageAsync<Models.ChatGroupAttachment>(
            pageSize, pagingCursorHelper.ToPagingState(pagingCursor), "WHERE chat_group_id = ?",
            [groupId]);
        
        var total = await DbMapper.FirstOrDefaultAsync<long>(
            "SELECT COUNT(*) FROM chat_group_attachments WHERE chat_group_id = ?;", groupId);
        
        var newCursor = pagingCursorHelper.ToPagingCursor(attachmentsPage.PagingState);
        return attachmentsPage
            .Select(_ => _.To<ChatGroupAttachment>(Mapper))
            .ToCursorPaged(newCursor, attachmentsPage.PagingState is not null, total);
    }
}
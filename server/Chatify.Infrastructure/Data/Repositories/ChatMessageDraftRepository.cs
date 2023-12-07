using AutoMapper;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Mappings;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Services;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

internal sealed class ChatMessageDraftRepository(IMapper mapper,
        Mapper dbMapper,
        IEntityChangeTracker changeTracker,
        string? idColumn = default)
    : BaseCassandraRepository<ChatMessageDraft, Models.ChatMessageDraft, Guid>(mapper, dbMapper, changeTracker,
            idColumn),
        IChatMessageDraftRepository
{
    public async Task<List<ChatMessageDraft>> AllForUser(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var drafts = await DbMapper
            .FetchListAsync<Models.ChatMessageDraft>(
                "SELECT * FROM chat_message_drafts WHERE user_id = ?", userId);
        return drafts.ToList<ChatMessageDraft>(Mapper);
    }

    public Task<ChatMessageDraft?> ForUserAndGroup(
        Guid userId,
        Guid groupId,
        CancellationToken cancellationToken = default)
        => DbMapper
            .FirstOrDefaultAsync<Models.ChatMessageDraft>(
                "SELECT * FROM chat_message_drafts WHERE user_id = ? AND chat_group_id = ?;", userId, groupId)!
            .ToAsyncNullable<Models.ChatMessageDraft, ChatMessageDraft>(Mapper);
}
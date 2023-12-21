using AutoMapper;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Caching.Extensions;
using Chatify.Infrastructure.Common.Mappings;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Chatify.Infrastructure.Data.Services;
using Chatify.Shared.Infrastructure.Common.Extensions;
using Humanizer;
using StackExchange.Redis;
using ChatGroupMember = Chatify.Domain.Entities.ChatGroupMember;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class ChatGroupMembersRepository(
        IMapper mapper,
        Mapper dbMapper,
        IDatabase cache,
        IEntityChangeTracker changeTracker)
    :
        BaseCassandraRepository<ChatGroupMember, Models.ChatGroupMember, Guid>(mapper, dbMapper, changeTracker,
            nameof(ChatGroupMember.ChatGroupId).Underscore()),
        IChatGroupMemberRepository

{
    public new async Task<ChatGroupMember> SaveAsync(
        ChatGroupMember entity,
        CancellationToken cancellationToken = default)
    {
        // Add new user to both database and cache set:
        var (member, _) = await (
            base.SaveAsync(entity, cancellationToken),
            cache.AddGroupMemberAsync(entity.ChatGroupId, entity.UserId)
        );

        return member;
    }

    public new async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var member = await GetAsync(id, cancellationToken);
        if ( member is null ) return false;

        // Delete member from both the database and the cache:
        var deleteTasks = new[]
        {
            base.DeleteAsync(member.Id, cancellationToken),
            cache.RemoveGroupMemberAsync(member.ChatGroupId, member.UserId)
        };

        var results = await Task.WhenAll(deleteTasks);
        return results.All(_ => _);
    }

    public async Task<ChatGroupMember?> ByGroupAndUser(
        Guid groupId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var member = await DbMapper.FirstOrDefaultAsync<Models.ChatGroupMember>(
            "WHERE chat_group_id = ? AND user_id = ? ALLOW FILTERING;",
            groupId,
            userId);

        return member?.To<ChatGroupMember>(Mapper);
    }

    public async Task<List<ChatGroupMember>?> ByGroup(
        Guid groupId,
        CancellationToken cancellationToken = default)
        => ( await DbMapper
                .FetchListAsync<Models.ChatGroupMember>(" WHERE chat_group_id = ?", groupId) )
            .ToList<ChatGroupMember>(Mapper);

    public async Task<List<Guid>?> UserIdsByGroup(
        Guid groupId,
        CancellationToken cancellationToken = default)
    {
        var memberIds = await DbMapper
            .FetchListAsync<Guid>("SELECT user_id FROM chat_group_members WHERE chat_group_id = ?", groupId);
        return memberIds;
    }

    public async Task<List<Guid>> GroupsIdsByUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var groupsIds = await DbMapper
            .FetchListAsync<Guid>("SELECT chat_group_id FROM chat_group_members_by_user_id WHERE user_id = ?", userId);
        return groupsIds;
    }

    public async Task<bool> Exists(
        Guid groupId,
        Guid userId,
        CancellationToken cancellationToken = default)
        => await dbMapper.FirstOrDefaultAsync<long>(
            "SELECT COUNT(*) FROM chat_group_members WHERE chat_group_id = ? AND user_id = ? ALLOW FILTERING;",
            groupId, userId) > 0;
}
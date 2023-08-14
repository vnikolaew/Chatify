﻿using AutoMapper;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Mappings;
using Chatify.Infrastructure.Data.Models;
using Humanizer;
using StackExchange.Redis;
using ChatGroupMember = Chatify.Domain.Entities.ChatGroupMember;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class ChatGroupMembersRepository :
    BaseCassandraRepository<ChatGroupMember, Models.ChatGroupMember, Guid>,
    IChatGroupMemberRepository

{
    private readonly IDatabase _cache;

    public ChatGroupMembersRepository(
        IMapper mapper, Mapper dbMapper, IDatabase cache)
        : base(mapper, dbMapper, nameof(ChatGroupMember.ChatGroupId).Underscore())
        => _cache = cache;

    private static RedisKey GetGroupMembersCacheKey(Guid groupId)
        => new($"groups:{groupId.ToString()}:members");

    public new async Task<ChatGroupMember> SaveAsync(
        ChatGroupMember entity,
        CancellationToken cancellationToken = default)
    {
        var groupKey = GetGroupMembersCacheKey(entity.ChatGroupId);
        var userKey = new RedisValue(entity.UserId.ToString());

        // Add new user to both database and cache set:
        var dbSaveTask = base.SaveAsync(entity, cancellationToken);
        var groupMemberByUserIdSaveTask = DbMapper
            .InsertAsync(new ChatGroupMemberByUser
            {
                UserId = entity.UserId,
                CreatedAt = entity.CreatedAt,
                ChatGroupId = entity.ChatGroupId,
                Id = entity.Id
            });
        var cacheSaveTask = _cache.ExecuteAsync("BF.ADD", groupKey, userKey);

        // (dbSaveTask, cacheSaveTask, groupMemberByUserIdSaveTask).when
        var saveTasks = new[] { dbSaveTask, cacheSaveTask, groupMemberByUserIdSaveTask };

        await Task.WhenAll(saveTasks).ConfigureAwait(false);
        return dbSaveTask.Result;
    }

    public new async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var member = await GetAsync(id, cancellationToken);
        if ( member is null ) return false;

        var groupKey = GetGroupMembersCacheKey(member.ChatGroupId);
        var userKey = new RedisValue(member.UserId.ToString());

        // Delete member from both the database and the cache:
        var groupMemberByUserIdDeleteTask =
            Task.Run(async () =>
            {
                try
                {
                    await DbMapper
                        .DeleteAsync<ChatGroupMemberByUser>(
                            " WHERE user_id = ? AND chat_group_id = ?",
                            member.UserId, member.ChatGroupId);
                    return true;
                }
                catch ( Exception )
                {
                    return false;
                }
            }, cancellationToken);

        var deleteTasks = new[]
        {
            base.DeleteAsync(member.Id, cancellationToken),
            groupMemberByUserIdDeleteTask,
            _cache.SetRemoveAsync(groupKey, userKey)
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

    public Task<List<ChatGroupMember>?> ByGroup(
        Guid groupId, CancellationToken cancellationToken = default)
        => DbMapper
            .FetchAsync<Models.ChatGroupMember>(" WHERE chat_group_id = ?", groupId)
            .ToAsyncNullable<IEnumerable<Models.ChatGroupMember>, List<ChatGroupMember>>(Mapper);

    public async Task<List<Guid>> GroupsIdsByUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var groupsIds = await DbMapper
            .FetchAsync<Guid>("SELECT chat_group_id FROM chat_group_members_by_user_id WHERE user_id = ?", userId);
        return groupsIds.ToList();
    }

    public async Task<bool> Exists(
        Guid groupId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var groupKey = GetGroupMembersCacheKey(groupId);
        var userKey = new RedisValue(userId.ToString());

        return ( bool )await _cache.ExecuteAsync("BF.EXISTS", groupKey, userKey);
    }
}
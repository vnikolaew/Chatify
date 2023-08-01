using AutoMapper;
using Chatify.Domain.Entities;
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

    public new async Task<ChatGroupMember> SaveAsync(
        ChatGroupMember entity,
        CancellationToken cancellationToken = default)
    {
        var groupKey = new RedisKey(entity.ChatGroupId.ToString());
        var userKey = new RedisValue(entity.UserId.ToString());

        // Add new user to both database and cache set:
        var dbSaveTask = base.SaveAsync(entity, cancellationToken);
        var cacheSaveTask = _cache.SetAddAsync(groupKey, userKey);
        
        var saveTasks = new Task[] { dbSaveTask, cacheSaveTask };

        await Task.WhenAll(saveTasks).ConfigureAwait(false);
        return dbSaveTask.Result;
    }

    public new async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var member = await GetAsync(id, cancellationToken);
        if (member is null) return false;

        var groupKey = new RedisKey(member.ChatGroupId.ToString());
        var userKey = new RedisValue(member.UserId.ToString());

        // Delete member from both the database and the cache:
        var deleteTasks = new[]
        {
            base.DeleteAsync(member.Id, cancellationToken),
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
        var member = await DbMapper.FirstOrDefaultAsync<ChatGroupMember>(
            "WHERE chat_group_id = ? AND user_id = ? ALLOW FILTERING",
            groupId,
            userId);

        return member;
    }

    public async Task<bool> Exists(
        Guid groupId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var groupKey = new RedisKey($"groups:{groupId}");
        var userKey = new RedisValue(userId.ToString());

        return await _cache.SetContainsAsync(groupKey, userKey);
    }
}
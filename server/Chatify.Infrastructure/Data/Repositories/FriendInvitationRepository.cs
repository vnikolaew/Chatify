using AutoMapper;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Mappings;
using Chatify.Infrastructure.Data.Services;
using Microsoft.Extensions.FileProviders;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class FriendInvitationRepository(IMapper mapper, Mapper dbMapper, IEntityChangeTracker changeTracker)
    :
        BaseCassandraRepository<FriendInvitation, Models.FriendInvitation, Guid>(mapper, dbMapper, changeTracker),
        IFriendInvitationRepository
{
    public async Task<List<FriendInvitation>> AllSentByUserAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var invites = await DbMapper
            .FetchAsync<Models.FriendInvitation>("WHERE inviter_id = ?", userId);

        return invites?
                   .Where(i => i.Status == ( sbyte )FriendInvitationStatus.Pending)
                   .ToList<FriendInvitation>(Mapper) ??
               new List<FriendInvitation>();
    }

    public async Task<FriendInvitation?> ForUsersAsync(
        Guid userOneId,
        Guid userTwoId,
        CancellationToken cancellationToken = default)
    {
        var dbTasks = new[]
        {
            DbMapper.FirstOrDefaultAsync<Models.FriendInvitation>(
                "WHERE inviter_id = ? AND invitee_id = ? ALLOW FILTERING;",
                userOneId, userTwoId),
            DbMapper.FirstOrDefaultAsync<Models.FriendInvitation>(
                "WHERE inviter_id = ? AND invitee_id = ? ALLOW FILTERING;",
                userTwoId, userOneId),
        };

        var friendInvites = await Task.WhenAll(dbTasks);
        return friendInvites.FirstOrDefault(_ => _ is not null) is { } invite
            ? Mapper.Map<FriendInvitation>(invite)
            : default;
    }

    public async Task<List<FriendInvitation>> AllSentToUserAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var invites = await DbMapper
            .FetchAsync<Models.FriendInvitation>("WHERE invitee_id = ?", userId);

        return invites?
                   .Where(i => i.Status == ( sbyte )FriendInvitationStatus.Pending)
                   .ToList<FriendInvitation>(Mapper) ??
               new List<FriendInvitation>();
    }
}
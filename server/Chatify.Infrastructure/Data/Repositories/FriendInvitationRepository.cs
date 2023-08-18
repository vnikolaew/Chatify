using AutoMapper;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Data.Services;
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
            .FetchAsync<FriendInvitation>("WHERE inviter_id = ?", userId);
        
        return invites?.Where(i => i.Status == (sbyte)FriendInvitationStatus.Pending).ToList() ??
               new List<FriendInvitation>();
    }

    public async Task<List<FriendInvitation>> AllSentToUserAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var invites = await DbMapper
            .FetchAsync<FriendInvitation>("WHERE invitee_id = ?", userId);
        
        return invites?.Where(i => i.Status == (sbyte)FriendInvitationStatus.Pending).ToList() ??
               new List<FriendInvitation>();
    }
}
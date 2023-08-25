using Chatify.Domain.Common;

namespace Chatify.Domain.Events.Friendships;

public class FriendInvitationAcceptedEvent : IDomainEvent
{
    public Guid FriendsRelationId { get; set; }
    
    public Guid InviteId { get; set; }
    
    public Guid InviterId { get; set; }
    
    public Guid InviteeId { get; set; }

    public Guid NewGroupId { get; set; }
    
    public DateTime Timestamp { get; set; }
    
}
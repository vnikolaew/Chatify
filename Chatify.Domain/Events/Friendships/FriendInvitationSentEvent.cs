using Chatify.Domain.Common;

namespace Chatify.Domain.Events.Friendships;

public class FriendInvitationSentEvent : IDomainEvent
{
    public Guid InviterId { get; set; }
    
    public Guid InviteeId { get; set; }
    
    public DateTime Timestamp { get; set; }
}
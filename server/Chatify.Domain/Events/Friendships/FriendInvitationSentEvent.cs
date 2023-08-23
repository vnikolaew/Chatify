using Chatify.Domain.Common;

namespace Chatify.Domain.Events.Friendships;

public class FriendInvitationSentEvent : IDomainEvent
{
    public Guid Id { get; set; }
    
    public Guid InviterId { get; set; }

    public string InviterUsername { get; set; } = default!;
    
    public Guid InviteeId { get; set; }
    
    public DateTime Timestamp { get; set; }
}
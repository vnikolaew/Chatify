using Chatify.Domain.Common;

namespace Chatify.Domain.Events.Friendships;

public sealed class UserUnfriendedEvent : IDomainEvent
{
    public Guid UserId { get; set; }
    
    public Guid UnfriendedById { get; set; }

    public string? Reason { get; set; }
    
    public DateTime Timestamp { get; set; }
}
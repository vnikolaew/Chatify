using Chatify.Domain.Common;

namespace Chatify.Domain.Events.Users;

public class UserChangedStatusEvent : IDomainEvent
{
    public Guid UserId { get; set; }
    
    public sbyte NewStatus { get; set; }

    public DateTime Timestamp { get; set; }
}
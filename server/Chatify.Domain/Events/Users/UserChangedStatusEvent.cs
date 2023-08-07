using Chatify.Domain.Common;
using Chatify.Domain.Entities;

namespace Chatify.Domain.Events.Users;

public class UserChangedStatusEvent : IDomainEvent
{
    public Guid UserId { get; set; }
    
    public UserStatus NewStatus { get; set; }

    public DateTime Timestamp { get; set; }
}
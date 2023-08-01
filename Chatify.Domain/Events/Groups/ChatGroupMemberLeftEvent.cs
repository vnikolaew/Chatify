using Chatify.Domain.Common;

namespace Chatify.Domain.Events.Groups;

public class ChatGroupMemberLeftEvent : IDomainEvent
{
    public Guid UserId { get; set; }

    public Guid GroupId { get; set; }
    
    public DateTime Timestamp { get; set; }
    
    public string? Reason { get; set; }
}
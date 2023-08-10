using Chatify.Domain.Common;

namespace Chatify.Domain.Events.JoinRequests;

public class ChatGroupJoinRequested : IDomainEvent
{
    public Guid RequestId { get; set; }
    
    public Guid UserId { get; set; }
    
    public Guid GroupId { get; set; }
    
    public DateTime Timestamp { get; set; }
}
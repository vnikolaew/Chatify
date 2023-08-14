using Chatify.Domain.Common;

namespace Chatify.Domain.Events.JoinRequests;

public class ChatGroupJoinRequestAccepted: IDomainEvent
{
    public Guid RequestId { get; set; }

    public Guid AcceptedById { get; set; }
    
    public Guid UserId { get; set; }

    public string Username { get; set; } = default!;
    
    public Guid GroupId { get; set; }
    
    public DateTime Timestamp { get; set; }
}
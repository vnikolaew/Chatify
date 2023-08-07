using Chatify.Domain.Common;

namespace Chatify.Domain.Events.Messages;

public class ChatMessageEditedEvent : IDomainEvent
{
    public Guid MessageId { get; set; }
    
    public Guid UserId { get; set; }
    
    public Guid GroupId { get; set; }

    public string NewContent { get; set; } = default!;
    
    public DateTime Timestamp { get; set; }
}
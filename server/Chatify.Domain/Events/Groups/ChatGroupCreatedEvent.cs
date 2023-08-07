using Chatify.Domain.Common;

namespace Chatify.Domain.Events.Groups;

public class ChatGroupCreatedEvent : IDomainEvent
{
    public Guid GroupId { get; set; }
    
    public Guid CreatorId { get; set; }
    
    public DateTime Timestamp { get; set; }

    public string Name { get; set; } = default!;
}
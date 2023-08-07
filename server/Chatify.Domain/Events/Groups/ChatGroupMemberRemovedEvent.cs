using Chatify.Domain.Common;

namespace Chatify.Domain.Events.Groups;

public class ChatGroupMemberRemovedEvent : IDomainEvent
{
    public Guid GroupId { get; set; }
    
    public Guid MemberId { get; set; }
    
    public DateTime Timestamp { get; set; }

    public Guid RemovedById { get; set; }
}
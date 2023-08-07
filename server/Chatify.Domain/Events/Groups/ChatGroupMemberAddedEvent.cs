using Chatify.Domain.Common;

namespace Chatify.Domain.Events.Groups;

public class ChatGroupMemberAddedEvent : IDomainEvent
{
    public Guid GroupId { get; set; }
    
    public Guid MemberId { get; set; }
    
    public Guid AddedById { get; set; }

    public string AddedByUsername { get; set; } = default!;
    
    public DateTime Timestamp { get; set; }
    
    public sbyte MembershipType { get; set; }
}
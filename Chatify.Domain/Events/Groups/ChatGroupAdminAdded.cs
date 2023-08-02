using Chatify.Domain.Common;

namespace Chatify.Domain.Events.Groups;

public class ChatGroupAdminAdded : IDomainEvent
{
    public Guid AdminId { get; set; }
    
    public Guid GroupId { get; set; }

    public DateTime Timestamp { get; set; }
}
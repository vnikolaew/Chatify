using Cassandra;

namespace Chatify.Infrastructure.Data.Models;

public class ChatGroup
{
    public TimeUuid Id { get; set; }
    
    public TimeUuid CreatorId { get; set; }
    
    public ISet<TimeUuid> AdminIds { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
}
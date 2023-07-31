using Cassandra;
using Metadata = System.Collections.Generic.IDictionary<string, string>;

namespace Chatify.Infrastructure.Data.Models;

public class MessageReaction
{
    public TimeUuid Id { get; set; }
    
    public TimeUuid MessageId { get; set; }
    
    public TimeUuid ChatGroupId { get; set; }
    
    public TimeUuid UserId { get; set; }
    
    public sbyte ReactionType { get; set; }
    
    public Metadata Metadata { get; set; } = new Dictionary<string, string>();
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset UpdatedAt { get; set; }
    
    public bool Updated { get; set; }
}
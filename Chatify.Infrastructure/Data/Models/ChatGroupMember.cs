using Cassandra;
using Metadata = System.Collections.Generic.IDictionary<string, string>;

namespace Chatify.Infrastructure.Data.Models;

public class ChatGroupMember
{
    public TimeUuid Id { get; set; }
    
    public TimeUuid UserId { get; set; }
    
    public TimeUuid ChatGroupId { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public Metadata Metadata { get; set; }
    
    public sbyte MembershipType { get; set; }
}
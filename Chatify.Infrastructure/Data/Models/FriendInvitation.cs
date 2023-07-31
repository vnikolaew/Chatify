using Cassandra;
using Metadata = System.Collections.Generic.IDictionary<string, string>;

namespace Chatify.Infrastructure.Data.Models;

public class FriendInvitation
{
    public TimeUuid Id { get; set; }
    
    public TimeUuid InviterId { get; set; }
    
    public TimeUuid InviteeId { get; set; }

    public Metadata Metadata { get; set; } = new Dictionary<string, string>();
    
    public sbyte Status { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset UpdatedAt { get; set; }
    
    public bool Updated { get; set; }
}
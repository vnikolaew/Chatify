using Cassandra;
using Metadata = System.Collections.Generic.IDictionary<string, string>;
using ReactionCounts = System.Collections.Generic.IDictionary<int, long>;

namespace Chatify.Infrastructure.Data.Models;

public class ChatMessage
{
    public TimeUuid Id { get; set; }
    
    public TimeUuid ChatGroupId { get; set; }

    public TimeUuid UserId { get; set; }
    
    public string Content { get; set; }

    private readonly HashSet<string> _attachments = new();

    public IEnumerable<string> Attachments => _attachments;
    
    public Metadata Metadata { get; set; } = new Dictionary<string, string>();

    public ReactionCounts ReactionCounts { get; set; } = new Dictionary<int, long>();
    
    // public long ReplyCount { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset UpdatedAt { get; set; }
    
    public bool Updated { get; set; }
}
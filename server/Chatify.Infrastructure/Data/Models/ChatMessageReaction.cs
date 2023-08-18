using Metadata = System.Collections.Generic.IDictionary<string, string>;
using ReactionCounts = System.Collections.Generic.IDictionary<short, long>;

namespace Chatify.Infrastructure.Data.Models;

public class ChatMessageReaction
{
    public Guid Id { get; set; }
    
    public Guid MessageId { get; set; }
    
    public Guid ChatGroupId { get; set; }
    
    public Guid UserId { get; set; }

    public string Username { get; set; } = default!;
    
    public sbyte ReactionType { get; set; }
    
    public Metadata Metadata { get; set; } = new Dictionary<string, string>();
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset UpdatedAt { get; set; }

    public ReactionCounts ReactionCounts { get; set; } = new Dictionary<short, long>();
    public bool Updated { get; set; }
}
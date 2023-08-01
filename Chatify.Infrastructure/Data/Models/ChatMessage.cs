using Cassandra.Mapping.Attributes;
using Chatify.Application.Common.Mappings;
using Metadata = System.Collections.Generic.IDictionary<string, string>;
using ReactionCounts = System.Collections.Generic.IDictionary<int, long>;

namespace Chatify.Infrastructure.Data.Models;

public class ChatMessage : IMapFrom<Domain.Entities.ChatMessage>
{
    [SecondaryIndex]
    public Guid Id { get; set; }
    
    public Guid ChatGroupId { get; set; }

    public Guid UserId { get; set; }

    public string Content { get; set; } = default!;

    protected readonly HashSet<string> _attachments = new();

    public IEnumerable<string> Attachments => _attachments;
    
    public Metadata Metadata { get; set; } = new Dictionary<string, string>();

    public ReactionCounts ReactionCounts { get; set; } = new Dictionary<int, long>();
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset? UpdatedAt { get; set; }

    public bool Updated => UpdatedAt.HasValue;
}
using Metadata = System.Collections.Generic.IDictionary<string, string>;
using ReactionCounts = System.Collections.Generic.IDictionary<int, long>;

namespace Chatify.Domain.Entities;

public record Media
{
    public Guid Id { get; set; }
    
    public string MediaUrl { get; set; } = default!;
    
    public string? FileName { get; init; }
    
    public string? Type { get; init; }
}


public class ChatMessage
{
    public Guid Id { get; set; }

    public Guid ChatGroupId { get; set; }

    public ChatGroup ChatGroup { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; }

    public string Content { get; set; } = default!;

    private readonly HashSet<Media> _attachments = new();

    public IReadOnlyCollection<Media> Attachments
    {
        get => _attachments.ToList().AsReadOnly();
        init => _attachments = value.ToHashSet();
    }

    public ReactionCounts ReactionCounts { get; set; } = new Dictionary<int, long>();

    public void AddAttachment(Media media) => _attachments.Add(media);

    public bool DeleteAttachment(Guid attachmentId)
        => _attachments.RemoveWhere(m => m.Id == attachmentId) > 0;

    public void IncrementReactionCount(sbyte reactionType)
    {
        if ( !ReactionCounts.ContainsKey(reactionType) )
        {
            ReactionCounts[reactionType] = 0;
        }

        ReactionCounts[reactionType]++;
    }

    public void DecrementReactionCount(sbyte reactionType)
    {
        if ( ReactionCounts.ContainsKey(reactionType) )
        {
            ReactionCounts[reactionType]--;
        }
    }

    public void ChangeReaction(sbyte from, sbyte to)
    {
        if ( ReactionCounts.ContainsKey(from) )
        {
            ReactionCounts[from]--;
        }

        if ( !ReactionCounts.ContainsKey(to) )
        {
            ReactionCounts[to] = 0;
        }

        ReactionCounts[to]++;
    }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;

    public Metadata Metadata { get; set; } = new Dictionary<string, string>();
}
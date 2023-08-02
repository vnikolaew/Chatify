using Metadata = System.Collections.Generic.IDictionary<string, string>;
using ReactionCounts = System.Collections.Generic.IDictionary<int, long>;

namespace Chatify.Domain.Entities;

public class ChatMessage
{
    public Guid Id { get; set; }

    public Guid ChatGroupId { get; set; }

    public ChatGroup ChatGroup { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; }

    public string Content { get; set; } = default!;

    private readonly HashSet<string> _attachments = new();

    public IReadOnlyCollection<string> Attachments
        => _attachments.ToList().AsReadOnly();

    public ReactionCounts ReactionCounts { get; set; } = new Dictionary<int, long>();

    public void IncrementReactionCount(sbyte reactionType)
    {
        if (!ReactionCounts.ContainsKey(reactionType))
        {
            ReactionCounts[reactionType] = 0;
        }

        ReactionCounts[reactionType]++;
    }
    
    public void DecrementReactionCount(sbyte reactionType)
    {
        if (ReactionCounts.ContainsKey(reactionType))
        {
            ReactionCounts[reactionType]--;
        }
    }

    public void ChangeReaction(sbyte from, sbyte to)
    {
        if (ReactionCounts.ContainsKey(from))
        {
            ReactionCounts[from]--;
        }

        if (!ReactionCounts.ContainsKey(to))
        {
            ReactionCounts[to] = 0;
        }

        ReactionCounts[to]++;
    }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;
}
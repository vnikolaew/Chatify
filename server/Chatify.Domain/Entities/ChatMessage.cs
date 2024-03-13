using Humanizer;
using Metadata = System.Collections.Generic.IDictionary<string, string>;
using ReactionCounts = System.Collections.Generic.IDictionary<long, long>;

namespace Chatify.Domain.Entities;

public class Media
{
    public Guid Id { get; set; }

    public string MediaUrl { get; set; } = default!;

    public string? FileName { get; set; }

    public string? Type { get; set; }
}

public class ChatMessage
{
    public Guid Id { get; set; }

    public Guid ChatGroupId { get; set; }

    public ChatGroup ChatGroup { get; set; }

    public Guid UserId { get; set; }

    public string Content { get; set; } = default!;

    private readonly HashSet<Media> _attachments = [];

    public Guid? ForwardedMessageId
    {
        get => Metadata.TryGetValue(nameof(ForwardedMessageId).Underscore(), out var id) &&
               Guid.TryParse(id, out var guiId)
            ? guiId
            : null;
        set => Metadata[nameof(ForwardedMessageId).Underscore()] = value?.ToString() ?? string.Empty;
    }

    public IReadOnlyCollection<Media> Attachments
    {
        get => _attachments.ToList().AsReadOnly();
        init => _attachments = value.ToHashSet();
    }

    public ReactionCounts ReactionCounts { get; init; } = new Dictionary<long, long>();

    public void AddAttachment(Media media) => _attachments.Add(media);

    public bool DeleteAttachment(Guid attachmentId)
        => _attachments.RemoveWhere(m => m.Id == attachmentId) > 0;

    public void IncrementReactionCount(long reactionType)
    {
        if ( !ReactionCounts.ContainsKey(reactionType) )
        {
            ReactionCounts[reactionType] = 0;
        }

        ReactionCounts[reactionType]++;
    }

    public void DecrementReactionCount(long reactionType)
    {
        if ( ReactionCounts.ContainsKey(reactionType) )
        {
            ReactionCounts[reactionType]--;
        }
    }

    public void ChangeReaction(long from, long to)
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
using Chatify.Domain.Common;
using Chatify.Domain.Entities;

namespace Chatify.Domain.Events.Messages;

public class ChatMessageDraftedEvent : IDomainEvent
{
    public Guid MessageId { get; set; }

    public Guid UserId { get; set; }

    public Guid GroupId { get; set; }

    public DateTime Timestamp { get; set; }

    public List<Media> Attachments { get; set; } = new();

    public string Content { get; set; } = default!;
}
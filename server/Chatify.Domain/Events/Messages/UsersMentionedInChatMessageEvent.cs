using Chatify.Domain.Common;

namespace Chatify.Domain.Events.Messages;

public class UsersMentionedInChatMessageEvent : IDomainEvent
{
    public Guid ChatGroupId { get; set; }

    public Guid MessageId { get; set; }

    public List<Guid> UsersMentionedIds { get; set; } = new();

    public DateTime Timestamp { get; set; }
}
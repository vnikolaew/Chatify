﻿using Chatify.Domain.Common;

namespace Chatify.Domain.Events.Messages;

public class ChatMessageUnreactedToEvent : IDomainEvent
{
    public Guid MessageId { get; set; }
    
    public Guid MessageReactionId { get; set; }

    public Guid UserId { get; set; }

    public Guid GroupId { get; set; }

    public DateTime Timestamp { get; set; }

    public long ReactionCode { get; set; }
}
namespace Chatify.Domain.Events.Messages;

public class ChatMessageReplyEditedEvent : ChatMessageEditedEvent 
{
    public Guid ReplyToId { get; set; }
}
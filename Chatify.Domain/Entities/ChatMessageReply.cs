namespace Chatify.Domain.Entities;

public class ChatMessageReply : ChatMessage
{
    public Guid ReplyToId { get; set; }
}
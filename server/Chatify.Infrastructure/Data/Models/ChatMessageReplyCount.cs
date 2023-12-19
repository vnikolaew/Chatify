namespace Chatify.Infrastructure.Data.Models;

public class ChatMessageReplyCount
{
    public const string TableName = "chat_messages_reply_count";

    public Guid Id { get; set; }
    
    public long ReplyCount { get; set; }
}
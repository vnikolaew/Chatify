using Cassandra;

namespace Chatify.Infrastructure.Data.Models;

public class ChatMessageReplyCount
{
    public TimeUuid Id { get; set; }
    
    public long ReplyCount { get; set; }
}
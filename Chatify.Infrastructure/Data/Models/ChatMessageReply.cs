using Metadata = System.Collections.Generic.IDictionary<string, string>;
using ReactionCounts = System.Collections.Generic.IDictionary<int, long>;

namespace Chatify.Infrastructure.Data.Models;

public class ChatMessageReply : ChatMessage
{
    public Guid ReplyToId { get; set; }
}
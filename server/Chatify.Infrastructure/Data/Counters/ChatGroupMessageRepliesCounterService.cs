using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Models;

namespace Chatify.Infrastructure.Data.Counters;

public sealed class ChatGroupMessageRepliesCounterService : BaseCounterService<ChatMessageReplyCount, Guid>
{
    public ChatGroupMessageRepliesCounterService(
        IMapper mapper) : base(c => c.ReplyCount, mapper)
    {
    }
}
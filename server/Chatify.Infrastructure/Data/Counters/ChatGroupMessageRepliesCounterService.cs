using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Models;

namespace Chatify.Infrastructure.Data.Counters;

public sealed class ChatGroupMessageRepliesCounterService(IMapper mapper)
    : BaseCounterService<ChatMessageReplyCount, Guid>(c => c.ReplyCount, mapper);
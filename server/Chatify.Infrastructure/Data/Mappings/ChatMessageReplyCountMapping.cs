using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Humanizer;

namespace Chatify.Infrastructure.Data.Mappings;

public class ChatMessageReplyCountMapping : Cassandra.Mapping.Mappings
{
    public ChatMessageReplyCountMapping()
    {
        For<ChatMessageReplyCount>()
            .TableName(nameof(ChatMessageReplyCount).Underscore())
            .KeyspaceName(Constants.KeyspaceName)
            .PartitionKey(rc => rc.Id)
            .UnderscoreColumn(rc => rc.Id)
            .UnderscoreColumn(rc => rc.ReplyCount, rc => rc.AsCounter());
    }
}
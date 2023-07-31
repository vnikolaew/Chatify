using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Humanizer;

namespace Chatify.Infrastructure.Data.Mappings;

public class ChatMessageMapping : Cassandra.Mapping.Mappings
{
    public ChatMessageMapping()
    {
        For<ChatMessage>()
            .TableName(nameof(ChatMessage).Underscore())
            .PartitionKey(cm => cm.Id)
            .ClusteringKey(
                new Tuple<string, SortOrder>(nameof(ChatMessage.CreatedAt), SortOrder.Descending),
                new Tuple<string, SortOrder>(nameof(ChatMessage.Id), SortOrder.Ascending)
            )
            .UnderscoreColumn(cm => cm.Id)
            .UnderscoreColumn(cm => cm.ChatGroupId)
            .UnderscoreColumn(cm => cm.Metadata)
            .UnderscoreColumn(cm => cm.UserId)
            .UnderscoreColumn(cm => cm.Content)
            .UnderscoreColumn(cm => cm.Attachments)
            .SetColumn<ChatMessage, IEnumerable<string>, string>(cm => cm.Attachments)
            .UnderscoreColumn(cm => cm.ReactionCounts)
            .UnderscoreColumn(cm => cm.CreatedAt)
            .UnderscoreColumn(cm => cm.UpdatedAt)
            .UnderscoreColumn(cm => cm.Updated);
    }
}
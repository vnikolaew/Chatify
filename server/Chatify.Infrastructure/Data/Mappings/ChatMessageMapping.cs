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
            .TableName(nameof(ChatMessage).Underscore().Pluralize())
            .PartitionKey(cm => cm.ChatGroupId)
            .ClusteringKey(
                new Tuple<string, SortOrder>(nameof(ChatMessage.CreatedAt).Underscore(), SortOrder.Descending)
            )
            .UnderscoreColumn(cm => cm.Id,
                cm => cm.WithSecondaryIndex())
            .UnderscoreColumn(cm => cm.ChatGroupId)
            .UnderscoreColumn(cm => cm.Metadata)
            .UnderscoreColumn(cm => cm.UserId)
            .UnderscoreColumn(cm => cm.Content)
            .UnderscoreColumn(cm => cm.Attachments)
            .SetColumn<ChatMessage, IEnumerable<Media>, Media>(m => m.Attachments)
            .UnderscoreColumn(cm => cm.Attachments, cm => cm.WithFrozenKey())
            .UnderscoreColumn(cm => cm.ReactionCounts)
            .UnderscoreColumn(cm => cm.CreatedAt)
            .UnderscoreColumn(cm => cm.UpdatedAt)
            .UnderscoreColumn(cm => cm.Updated);
    }
}
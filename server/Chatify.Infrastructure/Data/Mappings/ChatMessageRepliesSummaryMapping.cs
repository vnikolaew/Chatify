using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Humanizer;

namespace Chatify.Infrastructure.Data.Mappings;

public class ChatMessageRepliesSummaryMapping : Cassandra.Mapping.Mappings
{
    public ChatMessageRepliesSummaryMapping()
        => For<ChatMessageRepliesSummary>()
            .PartitionKey(s => s.ChatGroupId)
            .ClusteringKey(
                new Tuple<string, SortOrder>(nameof(ChatMessageRepliesSummary.CreatedAt), SortOrder.Descending),
                new Tuple<string, SortOrder>(nameof(ChatMessageRepliesSummary.MessageId), SortOrder.Ascending)
            )
            .TableName(nameof(ChatMessageRepliesSummary).Underscore())
            .UnderscoreColumn(s => s.ChatGroupId)
            .UnderscoreColumn(s => s.MessageId,
                cm => cm.WithSecondaryIndex())
            .UnderscoreColumn(s => s.Id)
            .UnderscoreColumn(s => s.CreatedAt)
            .UnderscoreColumn(s => s.Updated)
            .UnderscoreColumn(s => s.UpdatedAt)
            .UnderscoreColumn(s => s.Total)
            .UnderscoreColumn(s => s.ReplierIds)
            .UnderscoreColumn(s => s.ReplierInfos, cm => cm.WithFrozenKey());
}
using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Humanizer;

namespace Chatify.Infrastructure.Data.Mappings;

public class ChatMessageReactionMapping :Cassandra.Mapping.Mappings
{
    public ChatMessageReactionMapping()
    {
        For<ChatMessageReaction>()
            .TableName(nameof(ChatMessageReaction).Underscore().Pluralize())
            .PartitionKey(mr => mr.MessageId)
            .ClusteringKey(
                new Tuple<string, SortOrder>(nameof(ChatMessageReaction.CreatedAt).Underscore(), SortOrder.Descending),
                new Tuple<string, SortOrder>(nameof(ChatMessageReaction.Id).Underscore(), SortOrder.Ascending)
            )
            .UnderscoreColumn(mr => mr.Id,
                cm => cm.WithSecondaryIndex())
            .UnderscoreColumn(mr => mr.ReactionCounts,
                cm => cm.AsStatic())
            .UnderscoreColumn(mr => mr.MessageId)
            .UnderscoreColumn(mr => mr.ChatGroupId)
            .UnderscoreColumn(mr => mr.UserId)
            .UnderscoreColumn(mr => mr.ReactionCode)
            .UnderscoreColumn(mr => mr.Metadata)
            .UnderscoreColumn(mr => mr.CreatedAt)
            .UnderscoreColumn(mr => mr.UpdatedAt)
            .UnderscoreColumn(mr => mr.Updated);
    }
    
}
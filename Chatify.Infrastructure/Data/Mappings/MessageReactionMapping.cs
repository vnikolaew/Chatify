using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Humanizer;

namespace Chatify.Infrastructure.Data.Mappings;

public class MessageReactionMapping :Cassandra.Mapping.Mappings
{
    public MessageReactionMapping()
    {
        For<MessageReaction>()
            .TableName(nameof(MessageReaction).Underscore().Pluralize())
            .PartitionKey(mr => mr.MessageId)
            .ClusteringKey(
                new Tuple<string, SortOrder>(nameof(MessageReaction.CreatedAt), SortOrder.Descending),
                new Tuple<string, SortOrder>(nameof(MessageReaction.Id), SortOrder.Ascending)
            )
            .UnderscoreColumn(mr => mr.Id)
            .UnderscoreColumn(mr => mr.MessageId)
            .UnderscoreColumn(mr => mr.ChatGroupId)
            .UnderscoreColumn(mr => mr.UserId)
            .UnderscoreColumn(mr => mr.ReactionType)
            .UnderscoreColumn(mr => mr.Metadata)
            .UnderscoreColumn(mr => mr.CreatedAt)
            .UnderscoreColumn(mr => mr.UpdatedAt)
            .UnderscoreColumn(mr => mr.Updated);
    }
    
}
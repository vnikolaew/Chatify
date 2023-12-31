﻿using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Humanizer;

namespace Chatify.Infrastructure.Data.Mappings;

public class ChatMessageReplyMapping : Cassandra.Mapping.Mappings
{
    public ChatMessageReplyMapping()
    {
        For<ChatMessageReply>()
            .TableName(nameof(ChatMessageReply).Underscore().Pluralize())
            .PartitionKey(cm => cm.ReplyToId)
            .ClusteringKey(
                new Tuple<string, SortOrder>(nameof(ChatMessage.CreatedAt), SortOrder.Descending)
                // new Tuple<string, SortOrder>(nameof(ChatMessage.Id), SortOrder.Ascending)
            )
            .UnderscoreColumn(cm => cm.Id,
                cm => cm.WithSecondaryIndex())
            .UnderscoreColumn(cm => cm.ReplyToId)
            .UnderscoreColumn(cm => cm.ChatGroupId)
            .UnderscoreColumn(cm => cm.Metadata)
            .UnderscoreColumn(cm => cm.RepliesCount, cm => cm.AsStatic())
            .UnderscoreColumn(cm => cm.UserId)
            .UnderscoreColumn(cm => cm.Content)
            .UnderscoreColumn(cm => cm.Attachments)
            .SetColumn<ChatMessageReply, IEnumerable<Media>, Media>(cm => cm.Attachments)
            .UnderscoreColumn(cm => cm.ReactionCounts)
            .UnderscoreColumn(cm => cm.CreatedAt)
            .UnderscoreColumn(cm => cm.UpdatedAt)
            .UnderscoreColumn(cm => cm.Updated);
    }
    
}
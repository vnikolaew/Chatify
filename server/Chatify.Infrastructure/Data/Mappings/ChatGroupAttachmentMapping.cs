using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Humanizer;

namespace Chatify.Infrastructure.Data.Mappings;

public class ChatGroupAttachmentMapping : Cassandra.Mapping.Mappings
{
    public ChatGroupAttachmentMapping()
        => For<ChatGroupAttachment>()
            .TableName(nameof(ChatGroupAttachment).Pluralize().Underscore())
            .PartitionKey(a => a.ChatGroupId)
            .ClusteringKey(
                new Tuple<string, SortOrder>(nameof(ChatGroupAttachment.CreatedAt), SortOrder.Descending),
                new Tuple<string, SortOrder>(nameof(ChatGroupAttachment.AttachmentId), SortOrder.Ascending)
            )
            .UnderscoreColumn(a => a.ChatGroupId)
            .UnderscoreColumn(a => a.MediaInfo)
            .UnderscoreColumn(a => a.AttachmentId)
            .UnderscoreColumn(a => a.UserId)
            .UnderscoreColumn(a => a.Username)
            .UnderscoreColumn(a => a.CreatedAt)
            .UnderscoreColumn(a => a.UpdatedAt);
}
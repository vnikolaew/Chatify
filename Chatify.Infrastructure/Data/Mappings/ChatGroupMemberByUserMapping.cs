using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Humanizer;

namespace Chatify.Infrastructure.Data.Mappings;

public class ChatGroupMemberByUserMapping : Cassandra.Mapping.Mappings
{
    public ChatGroupMemberByUserMapping()
        => For<ChatGroupMemberByUser>()
            .TableName(nameof(ChatGroupMemberByUser).Underscore())
            .PartitionKey(m => m.UserId)
            .ClusteringKey(
                new Tuple<string, SortOrder>(nameof(ChatGroupMember.CreatedAt), SortOrder.Descending),
                new Tuple<string, SortOrder>(nameof(ChatGroupMember.ChatGroupId), SortOrder.Ascending)
            )
            .UnderscoreColumn(cgm => cgm.Id)
            .UnderscoreColumn(cgm => cgm.UserId)
            .UnderscoreColumn(cgm => cgm.ChatGroupId)
            .UnderscoreColumn(cgm => cgm.CreatedAt);
}
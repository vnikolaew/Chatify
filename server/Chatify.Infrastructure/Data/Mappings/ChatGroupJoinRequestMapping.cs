using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;

namespace Chatify.Infrastructure.Data.Mappings;

public class ChatGroupJoinRequestMapping : Cassandra.Mapping.Mappings
{
    public ChatGroupJoinRequestMapping()
        => For<ChatGroupJoinRequest>()
            .PartitionKey(r => r.ChatGroupId)
            .ClusteringKey(new Tuple<string, SortOrder>[]
            {
                new(nameof(ChatGroupJoinRequest.CreatedAt), SortOrder.Descending),
                new(nameof(ChatGroupJoinRequest.UserId), SortOrder.Ascending)
            })
            .UnderscoreColumn(r => r.ChatGroupId)
            .UnderscoreColumn(r => r.Id,
                cm => cm.WithSecondaryIndex())
            .UnderscoreColumn(r => r.UserId)
            .UnderscoreColumn(r => r.CreatedAt)
            .UnderscoreColumn(r => r.UpdatedAt)
            .UnderscoreColumn(r => r.Status);
}
using Cassandra;
using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Humanizer;

namespace Chatify.Infrastructure.Data.Mappings;

public class ChatGroupMapping : Cassandra.Mapping.Mappings
{
    public ChatGroupMapping()
    {
        For<ChatGroup>()
            .TableName(nameof(ChatGroup).Underscore())
            .KeyspaceName(Constants.KeyspaceName)
            .PartitionKey(cg => cg.Id)
            .ClusteringKey(cg => cg.CreatedAt, SortOrder.Descending)
            .SetColumn<ChatGroup, ISet<TimeUuid>, TimeUuid>(g => g.AdminIds)
            .UnderscoreColumn(g => g.Id)
            .UnderscoreColumn(g => g.CreatorId)
            .UnderscoreColumn(g => g.AdminIds)
            .UnderscoreColumn(g => g.CreatedAt);
        // .UnderscoreColumn(g => g.MembersCount);
    }
}
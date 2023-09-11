using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using FastDeepCloner;
using Humanizer;

namespace Chatify.Infrastructure.Data.Mappings;

public class ChatGroupMapping : Cassandra.Mapping.Mappings
{
    public ChatGroupMapping()
        => For<ChatGroup>()
            .TableName(nameof(ChatGroup).Underscore().Pluralize())
            .KeyspaceName(Constants.KeyspaceName)
            .PartitionKey(cg => cg.Id)
            .ClusteringKey(cg => cg.CreatedAt)
            .SetColumn<ChatGroup, ISet<Guid>, Guid>(g => g.AdminIds)
            .UnderscoreColumn(g => g.Id)
            .UnderscoreColumn(g => g.PinnedMessages, cm => cm.WithFrozenValue())
            .UnderscoreColumn(g => g.CreatorId)
            .UnderscoreColumn(g => g.AdminIds)
            .UnderscoreColumn(g => g.CreatedAt)
            .UnderscoreColumn(g => g.Name)
            .UnderscoreColumn(g => g.Metadata)
            .UnderscoreColumn(g => g.UpdatedAt)
            .UnderscoreColumn(g => g.About)
            .UnderscoreColumn(g => g.Picture);
    // .UnderscoreColumn(g => g.MembersCount);
}
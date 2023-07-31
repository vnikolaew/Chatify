using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Humanizer;
using LanguageExt.TypeClasses;

namespace Chatify.Infrastructure.Data.Mappings;

public class FriendsRelationMapping : Cassandra.Mapping.Mappings
{
    public FriendsRelationMapping()
    {
        For<FriendsRelation>()
            .TableName(nameof(FriendsRelation).Underscore())
            .KeyspaceName(Constants.KeyspaceName)
            .PartitionKey(fr => fr.FriendOneId)
            .ClusteringKey(fr => fr.CreatedAt, SortOrder.Descending)
            .UnderscoreColumn(fr => fr.FriendOneId)
            .UnderscoreColumn(fr => fr.FriendTwoId)
            .UnderscoreColumn(fr => fr.CreatedAt);
    }
}
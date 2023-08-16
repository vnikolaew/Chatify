using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;

namespace Chatify.Infrastructure.Data.Mappings;

public class FriendsRelationMapping : Cassandra.Mapping.Mappings
{
    private const string TableName = "friends";

    public FriendsRelationMapping()
    {
        For<FriendsRelation>()
            .TableName(TableName)
            .KeyspaceName(Constants.KeyspaceName)
            .PartitionKey(fr => fr.FriendOneId)
            .ClusteringKey(fr => fr.CreatedAt, SortOrder.Descending)
            .UnderscoreColumn(fr => fr.FriendOneId)
            .UnderscoreColumn(fr => fr.FriendTwoId)
            .UnderscoreColumn(fr => fr.GroupId)
            .UnderscoreColumn(fr => fr.CreatedAt);
    }
}
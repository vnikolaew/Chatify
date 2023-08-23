using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Humanizer;

namespace Chatify.Infrastructure.Data.Mappings;

public sealed class UserNotificationMapping : Cassandra.Mapping.Mappings
{
    public UserNotificationMapping()
        => For<UserNotification>()
            .TableName(nameof(UserNotification).Pluralize().Underscore())
            .PartitionKey(n => n.UserId)
            .ClusteringKey(n => n.CreatedAt, SortOrder.Descending)
            .UnderscoreColumn(n => n.UserId)
            .UnderscoreColumn(n => n.Metadata)
            .UnderscoreColumn(n => n.UpdatedAt)
            .UnderscoreColumn(n => n.Type)
            .UnderscoreColumn(n => n.Read)
            .UnderscoreColumn(n => n.Summary)
            .UnderscoreColumn(n => n.Id, cm => cm.WithSecondaryIndex());
}
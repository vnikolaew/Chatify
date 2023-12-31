﻿using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Humanizer;

namespace Chatify.Infrastructure.Data.Mappings;

public class FriendInvitationMapping :  Cassandra.Mapping.Mappings
{
    public FriendInvitationMapping()
    {
        For<FriendInvitation>()
            .TableName(nameof(FriendInvitation).Pluralize().Underscore())
            .PartitionKey(fi => fi.InviterId)
            .ClusteringKey(fi => fi.CreatedAt,
                SortOrder.Descending)
            .UnderscoreColumn(fi => fi.Id,
                cm => cm.WithSecondaryIndex())
            .UnderscoreColumn(fi => fi.InviterId)
            .UnderscoreColumn(fi => fi.InviteeId,
                cm => cm.WithSecondaryIndex())
            .UnderscoreColumn(fi => fi.Metadata)
            .UnderscoreColumn(fi => fi.Status)
            .UnderscoreColumn(fi => fi.CreatedAt)
            .UnderscoreColumn(fi => fi.Updated)
            .UnderscoreColumn(fi => fi.UpdatedAt);
    }
}
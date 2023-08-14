﻿using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Humanizer;
using LanguageExt;

namespace Chatify.Infrastructure.Data.Mappings;

public class ChatGroupMemberMapping : Cassandra.Mapping.Mappings
{
    public ChatGroupMemberMapping()
    {
        For<ChatGroupMember>()
            .TableName(nameof(ChatGroupMember).Underscore().Pluralize())
            .PartitionKey(cgm => cgm.ChatGroupId)
            .ClusteringKey(
                new Tuple<string, SortOrder>(nameof(ChatGroupMember.CreatedAt), SortOrder.Descending),
                new Tuple<string, SortOrder>(nameof(ChatGroupMember.Id), SortOrder.Ascending)
            )
            .UnderscoreColumn(cgm => cgm.Id)
            .UnderscoreColumn(cgm => cgm.UserId)
            .UnderscoreColumn(cgm => cgm.Username)
            .UnderscoreColumn(cgm => cgm.ChatGroupId)
            .UnderscoreColumn(cgm => cgm.CreatedAt)
            .UnderscoreColumn(cgm => cgm.Metadata)
            .UnderscoreColumn(cgm => cgm.MembershipType);
    }
}
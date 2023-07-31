using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Models;
using Humanizer;

namespace Chatify.Infrastructure.Data.Mappings;

public class ChatGroupMembersCountMapping : Cassandra.Mapping.Mappings
{
    public ChatGroupMembersCountMapping()
    {
        For<ChatGroupMembersCount>()
            .TableName(nameof(ChatGroupMembersCount).Underscore())
            .PartitionKey(mc => mc.Id)
            .UnderscoreColumn(mc => mc.Id)
            .UnderscoreColumn(mc => mc.MembersCount)
            .Column(mc => mc.MembersCount, mc => mc.AsCounter());
    }
}
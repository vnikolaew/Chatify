using Cassandra;

namespace Chatify.Infrastructure.Data.Models;

public class ChatGroupMembersCount
{
    public TimeUuid Id { get; set; }
    
    public long MembersCount { get; set; }
}
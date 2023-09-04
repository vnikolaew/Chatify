namespace Chatify.Infrastructure.Data.Models;

public class ChatGroupMembersCount
{
    public Guid ChatGroupId { get; set; }
    
    public long MembersCount { get; set; }
}
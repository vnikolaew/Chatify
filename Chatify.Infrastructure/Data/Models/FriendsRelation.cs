using Cassandra;

namespace Chatify.Infrastructure.Data.Models;

public class FriendsRelation
{
    public TimeUuid FriendOneId { get; set; }
    
    public TimeUuid FriendTwoId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
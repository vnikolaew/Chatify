namespace Chatify.Domain.Entities;

public class FriendsRelation
{
    public Guid Id { get; set; }
    
    public Guid FriendOneId { get; set; }
    
    public Guid FriendTwoId { get; set; }

    public Guid GroupId { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
}
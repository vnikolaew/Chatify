namespace Chatify.Domain.Entities;

public class ChatGroupMember
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public string Username { get; set; } = default!;

    public User User { get; set; }
    
    public Guid ChatGroupId { get; set; }

    public ChatGroup ChatGroup { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
    
    public sbyte MembershipType { get; set; }
}
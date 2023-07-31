namespace Chatify.Domain.Entities;

public class ChatGroupMember
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public Guid ChatGroupId { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
    
    public sbyte MembershipType { get; set; }
}
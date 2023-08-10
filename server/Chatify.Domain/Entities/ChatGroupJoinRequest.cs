namespace Chatify.Domain.Entities;

public class ChatGroupJoinRequest
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public Guid ChatGroupId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public sbyte Status { get; set; }
}
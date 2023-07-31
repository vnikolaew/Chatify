namespace Chatify.Domain.Entities;

public class FriendInvitation
{
    public Guid Id { get; set; }
    
    public Guid InviterId { get; set; }
    
    public Guid InviteeId { get; set; }

    public sbyte Status { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    public DateTimeOffset? UpdatedAt { get; set; } = default;
}
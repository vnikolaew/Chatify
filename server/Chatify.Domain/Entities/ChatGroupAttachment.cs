namespace Chatify.Domain.Entities;

public class ChatGroupAttachment
{
    public Guid ChatGroupId { get; set; }

    public Guid AttachmentId { get; set; }

    public Guid UserId { get; set; }

    public string Username { get; set; } = default!;

    public Media MediaInfo { get; set; } = default!;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }
}
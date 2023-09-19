namespace Chatify.Domain.Entities;

public class MessageRepliersInfo
{
    public Guid Id { get; set; }
    public Guid ChatGroupId { get; set; }

    public Guid MessageId { get; set; }

    public DateTime? LastUpdatedAt { get; set; } = default;
    
    public DateTimeOffset CreatedAt { get; set; }

    public long Total { get; set; }

    public ISet<MessageReplierInfo> ReplierInfos { get; set; } = new HashSet<MessageReplierInfo>();
}

public record MessageReplierInfo(
    Guid UserId,
    string Username,
    string ProfilePictureUrl
);
namespace Chatify.Domain.Entities;
using Metadata = Dictionary<string, string>;

public class UserNotification
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public User? User { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    public DateTimeOffset? UpdatedAt { get; set; }

    public UserNotificationType Type { get; set; }

    public Metadata Metadata { get; set; } = new();

    public string? Summary { get; set; }

    public bool Read { get; set; }
}

public enum UserNotificationType : sbyte
{
    Unspecified,
    IncomingFriendInvite,
    AcceptedFriendInvite,
    DeclinedFriendInvite,
    UserMention
}

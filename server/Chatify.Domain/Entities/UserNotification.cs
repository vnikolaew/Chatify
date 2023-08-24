namespace Chatify.Domain.Entities;

using Metadata = Dictionary<string, string>;

public abstract class UserNotification
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public User? User { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    public DateTimeOffset? UpdatedAt { get; set; }

    public abstract UserNotificationType Type { get; set; }

    public UserNotificationMetadata? Metadata { get; set; }

    public string? Summary { get; set; }

    public bool Read { get; set; } = false;
}

public class IncomingFriendInvitationNotification : UserNotification
{
    public Guid InviteId { get; set; }

    public override UserNotificationType Type
    {
        get => UserNotificationType.IncomingFriendInvite;
        set { }
    }
}

public enum UserNotificationType : sbyte
{
    Unspecified,
    IncomingFriendInvite,
    AcceptedFriendInvite,
    DeclinedFriendInvite,
    UserMention
}

public class UserNotificationMetadata
{
    public Media? UserMedia { get; set; }
}
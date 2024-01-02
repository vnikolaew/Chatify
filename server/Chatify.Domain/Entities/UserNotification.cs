namespace Chatify.Domain.Entities;

public class UserNotification
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    public DateTimeOffset? UpdatedAt { get; set; }

    public virtual UserNotificationType Type { get; set; }

    public UserNotificationMetadata? Metadata { get; set; }

    public virtual string? Summary { get; set; }

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

public class AcceptedFriendInvitationNotification : UserNotification
{
    public Guid InviteId { get; set; }

    public Guid ChatGroupId { get; set; }
    public Guid InviterId { get; set; }
    
    public override UserNotificationType Type
    {
        get => UserNotificationType.AcceptedFriendInvite;
        set { }
    }
}

public class DeclinedFriendInvitationNotification
    : AcceptedFriendInvitationNotification
{
    public override UserNotificationType Type
    {
        get => UserNotificationType.DeclinedFriendInvite;
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
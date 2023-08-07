namespace Chatify.Infrastructure.Messages.Hubs.Models.Server;

public sealed record ChatGroupMemberStartedTyping(
    Guid ChatGroupId,
    Guid UserId,
    string Username,
    DateTime Timestamp
);

public sealed record ChatGroupMemberStoppedTyping(
    Guid ChatGroupId,
    Guid UserId,
    string Username,
    DateTime Timestamp
);

public sealed record UserStatusChanged(
    Guid UserId,
    string Username,
    sbyte NewStatus,
    DateTime Timestamp
);

public sealed record ChatGroupMemberJoined(
    Guid ChatGroupId,
    Guid UserId,
    string Username,
    DateTime Timestamp
);

public sealed record ChatGroupMemberLeft(
    Guid ChatGroupId,
    Guid UserId,
    string Username,
    DateTime Timestamp
);

public sealed record ChatGroupMemberRemoved(
    Guid ChatGroupId,
    Guid RemovedById,
    Guid UserId,
    string Username,
    DateTime Timestamp
);

public sealed record ChatGroupMessageRemoved(
    Guid ChatGroupId,
    Guid MessageId,
    Guid UserId,
    string Username,
    DateTime Timestamp
);

public sealed record ChatGroupMessageEdited(
    Guid ChatGroupId,
    Guid MessageId,
    Guid UserId,
    string NewContent,
    DateTime Timestamp,
    Dictionary<string, string>? Metadata = default
);

public sealed record ChatGroupMessageReactedTo(
    Guid ChatGroupId,
    Guid MessageId,
    Guid MessageReactionId,
    Guid UserId,
    sbyte ReactionType,
    DateTime Timestamp,
    Dictionary<string, string>? Metadata = default
);

public sealed record ChatGroupMessageUnReactedTo(
    Guid ChatGroupId,
    Guid MessageId,
    Guid MessageReactionId,
    Guid UserId,
    sbyte ReactionType,
    DateTime Timestamp,
    Dictionary<string, string>? Metadata = default
);

public sealed record ReceiveFriendInvitation(
    Guid InviterId,
    DateTime Timestamp,
    Dictionary<string, string>? Metadata = default
);

public record FriendInvitationResponded(
    Guid InviteeId,
    DateTime Timestamp,
    Dictionary<string, string>? Metadata
);

public sealed record FriendInvitationAccepted(
    Guid InviteeId,
    DateTime Timestamp,
    Dictionary<string, string>? Metadata = default
) : FriendInvitationResponded(InviteeId, Timestamp, Metadata);

public sealed record FriendInvitationDeclined(
    Guid InviteeId,
    DateTime Timestamp,
    Dictionary<string, string>? Metadata = default
) : FriendInvitationResponded(InviteeId, Timestamp, Metadata);

public sealed record AddedToChatGroup(
    Guid ChatGroupId,
    Guid AddedById,
    string AddedByUsername,
    DateTime Timestamp,
    Dictionary<string, string>? Metadata = default
);

public sealed record RemovedFromChatGroup(
    Guid ChatGroupId,
    Guid RemovedById,
    string RemovedByUsername,
    DateTime Timestamp,
    Dictionary<string, string>? Metadata = default
);

public sealed record ChatGroupNewAdminAdded(
    Guid ChatGroupId,
    Guid AdminId,
    DateTime Timestamp,
    Dictionary<string, string>? Metadata = default
);
namespace Chatify.Infrastructure.Messages.Hubs.Models.Server;

internal sealed record ChatGroupMemberStartedTyping(
    Guid ChatGroupId,
    Guid UserId,
    string Username,
    DateTime Timestamp
);

internal sealed record ChatGroupMemberStoppedTyping(
    Guid ChatGroupId,
    Guid UserId,
    string Username,
    DateTime Timestamp
);

internal sealed record UserStatusChanged(
    Guid UserId,
    string Username,
    sbyte NewStatus,
    DateTime Timestamp
);

internal sealed record ChatGroupMemberJoined(
    Guid ChatGroupId,
    Guid UserId,
    string Username,
    DateTime Timestamp
);

internal sealed record ChatGroupMemberLeft(
    Guid ChatGroupId,
    Guid UserId,
    string Username,
    DateTime Timestamp
);

internal sealed record ChatGroupMemberRemoved(
    Guid ChatGroupId,
    Guid RemovedById,
    Guid UserId,
    string Username,
    DateTime Timestamp
);

internal sealed record ChatGroupMessageRemoved(
    Guid ChatGroupId,
    Guid MessageId,
    Guid UserId,
    string Username,
    DateTime Timestamp
);

internal sealed record ChatGroupMessageEdited(
    Guid ChatGroupId,
    Guid MessageId,
    Guid UserId,
    string NewContent,
    DateTime Timestamp,
    Dictionary<string, string>? Metadata = default
);

internal sealed record ChatGroupMessageReactedTo(
    Guid ChatGroupId,
    Guid MessageId,
    Guid MessageReactionId,
    Guid UserId,
    sbyte ReactionType,
    DateTime Timestamp,
    Dictionary<string, string>? Metadata = default
);

internal sealed record ChatGroupMessageUnReactedTo(
    Guid ChatGroupId,
    Guid MessageId,
    Guid MessageReactionId,
    Guid UserId,
    sbyte ReactionType,
    DateTime Timestamp,
    Dictionary<string, string>? Metadata = default
);

internal sealed record ReceiveFriendInvitation(
    Guid InviterId,
    DateTime Timestamp,
    Dictionary<string, string>? Metadata = default
);

internal record FriendInvitationResponded(
    Guid InviteeId,
    DateTime Timestamp,
    Dictionary<string, string>? Metadata
);

internal sealed record FriendInvitationAccepted(
    Guid InviteeId,
    DateTime Timestamp,
    Dictionary<string, string>? Metadata = default
) : FriendInvitationResponded(InviteeId, Timestamp, Metadata);

internal sealed record FriendInvitationDeclined(
    Guid InviteeId,
    DateTime Timestamp,
    Dictionary<string, string>? Metadata = default
) : FriendInvitationResponded(InviteeId, Timestamp, Metadata);

internal sealed record AddedToChatGroup(
    Guid ChatGroupId,
    Guid AddedById,
    string AddedByUsername,
    DateTime Timestamp,
    Dictionary<string, string>? Metadata = default
);

internal sealed record RemovedFromChatGroup(
    Guid ChatGroupId,
    Guid RemovedById,
    string RemovedByUsername,
    DateTime Timestamp,
    Dictionary<string, string>? Metadata
);
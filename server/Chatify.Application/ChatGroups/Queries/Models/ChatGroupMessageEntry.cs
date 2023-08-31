using Chatify.Domain.Entities;

namespace Chatify.Application.ChatGroups.Queries.Models;

public record MessageSenderInfoEntry(
    Guid UserId,
    string Username,
    string ProfilePictureUrl
);

public record MessageReplierInfoEntry(
    Guid UserId,
    string Username,
    string ProfilePictureUrl
) : MessageSenderInfoEntry(UserId, Username, ProfilePictureUrl);

public record MessageRepliersInfoEntry(
    long Total,
    DateTime? LastUpdatedAt,
    List<MessageReplierInfoEntry> ReplierInfos
);

public record UserMessageReaction(long ReactionCode);

public record ChatGroupMessageEntry
{
    public required ChatMessage Message { get; set; }
    
    public ChatMessage? ForwardedMessage { get; set; }

    public MessageSenderInfoEntry SenderInfo { get; set; }
    
    public UserMessageReaction? UserReaction { get; set; }

    public required MessageRepliersInfoEntry RepliersInfo { get; set; }
};
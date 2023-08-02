namespace Chatify.Infrastructure.Messages.Hubs.Models.Server;

internal sealed record ReceiveGroupChatMessage(
    Guid ChatGroupId,
    Guid SenderId,
    Guid MessageId,
    string SenderUsername,
    string Content,
    DateTime Timestamp
    );
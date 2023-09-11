using System.ComponentModel.DataAnnotations;
using Chatify.Domain.Entities;

namespace Chatify.Infrastructure.Messages.Hubs.Models.Client;

public sealed record SubscribeToAllChatGroupMessagesRequest;

public sealed record SubscribeToChatGroupMessagesRequest(
    [Required] Guid ChatGroupId);
    
public sealed record ReceiveGroupChatMessage(
    Guid ChatGroupId,
    Guid SenderId,
    Guid MessageId,
    string SenderUsername,
    string Content,
    DateTime Timestamp,
    IEnumerable<Media>? attachments = default);

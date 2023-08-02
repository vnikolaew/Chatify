using System.ComponentModel.DataAnnotations;

namespace Chatify.Infrastructure.Messages.Hubs.Models.Client;

internal sealed record SubscribeToAllChatGroupMessagesRequest;

internal sealed record SubscribeToChatGroupMessagesRequest(
    [Required] Guid ChatGroupId);
using Chatify.Domain.Entities;

namespace Chatify.Application.ChatGroups.Queries.Models;

public record MessageReplierInfoResponseModel(
    Guid UserId,
    string Username,
    string ProfilePictureUrl
);

public record MessageRepliersInfoResponseModel(
    long Total,
    DateTime? LastUpdatedAt,
    List<MessageReplierInfoResponseModel> ReplierInfos
);


public record ChatGroupMessageResponseModel(
    ChatMessage Message,
    MessageRepliersInfoResponseModel RepliersInfo
);
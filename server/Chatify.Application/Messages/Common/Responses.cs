using Chatify.Application.Common.Models;

namespace Chatify.Application.Messages.Common;

public record MessageNotFoundError(Guid MessageId);

public record UserIsNotMemberError(Guid UserId, Guid ChatGroupId)
    : BaseError("User is not chat group member.");

public record ChatGroupNotFoundError;

public record UserIsNotMessageSenderError(Guid MessageId, Guid UserId)
    : BaseError("User is not a sender of this chat message.");

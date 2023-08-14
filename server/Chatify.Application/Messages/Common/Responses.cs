namespace Chatify.Application.Messages.Common;

public record MessageNotFoundError(Guid MessageId);

public record UserIsNotMemberError(Guid UserId, Guid ChatGroupId);

public record ChatGroupNotFoundError;

public record UserIsNotMessageSenderError(Guid MessageId, Guid UserId);

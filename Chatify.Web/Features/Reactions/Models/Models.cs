using System.ComponentModel.DataAnnotations;
using Chatify.Application.Messages.Reactions.Commands;

namespace Chatify.Web.Features.Reactions.Models;

public static class Models
{
    public record ReactToChatMessageRequest(
        [Required] Guid MessageId,
        [Required] Guid GroupId,
        [Required] sbyte ReactionType
    )
    {
        public ReactToChatMessage ToCommand()
            => new(MessageId, GroupId, ReactionType);
        
        public ReactToChatMessageReply ToReplyCommand()
            => new(MessageId, GroupId, ReactionType);
    }

    public record UnreactToChatMessageRequest(
        [Required] Guid MessageReactionId,
        [Required] Guid MessageId,
        [Required] Guid GroupId)
    {
        public UnreactToChatMessage ToCommand()
            => new(MessageReactionId, MessageId, GroupId);

        public UnreactToChatMessageReply ToReplyCommand()
            => new(MessageReactionId, MessageId, GroupId);
    }
}
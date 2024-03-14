using System.ComponentModel.DataAnnotations;
using Chatify.Application.Messages.Reactions.Commands;

namespace Chatify.Web.Features.Reactions.Models;

public static class Models
{
    public record ReactToChatMessageRequest
    {
        [Required] public Guid MessageId { get; init; }

        [Required] public Guid GroupId { get; init; }

        [Required] public long ReactionType { get; init; }

        public ReactToChatMessage ToCommand()
            => new(MessageId, GroupId, ReactionType);

        public ReactToChatMessageReply ToReplyCommand()
            => new(MessageId, GroupId, ReactionType);
    }

    public record UnreactToChatMessageRequest()
    {
        [Required] public Guid MessageId { get; init; }

        [Required] public Guid GroupId { get; init; }

        [Required] public Guid MessageReactionId { get; init; }
        
        public UnreactToChatMessage ToCommand()
            => new(MessageReactionId, MessageId, GroupId);

        public UnreactToChatMessageReply ToReplyCommand()
            => new(MessageReactionId, MessageId, GroupId);
    }
}
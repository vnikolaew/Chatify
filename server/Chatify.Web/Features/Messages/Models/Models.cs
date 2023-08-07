using System.ComponentModel.DataAnnotations;
using Chatify.Application.ChatGroups.Queries;
using Chatify.Application.Common.Models;
using Chatify.Application.Messages.Commands;
using Chatify.Application.Messages.Replies.Commands;
using Chatify.Application.Messages.Replies.Queries;

namespace Chatify.Web.Features.Messages.Models;

public static class Models
{
    public sealed record SendGroupChatMessageRequest(
        [Required] Guid ChatGroupId,
        [Required] string Content,
        IEnumerable<IFormFile>? Files,
        Dictionary<string, string>? Metadata = default)
    {
        public SendGroupChatMessage ToCommand()
            => new(ChatGroupId, Content,
                Files?.Select(f => new InputFile
                {
                    Data = f.OpenReadStream(),
                    FileName = f.FileName
                }));
    };

    public sealed record SendGroupChatMessageReplyRequest(
        [Required] Guid ChatGroupId,
        [Required] Guid ReplyToId,
        [Required] string Content,
        IEnumerable<IFormFile>? Files,
        Dictionary<string, string>? Metadata = default)
    {
        public ReplyToChatMessage ToCommand()
            => new(ChatGroupId, ReplyToId, Content,
                Files?.Select(f => new InputFile
                {
                    Data = f.OpenReadStream(),
                    FileName = f.FileName
                }));
    };

    public sealed record EditGroupChatMessageRequest(
        [Required] Guid GroupId,
        [Required] Guid MessageId,
        [Required] string NewContent
    )
    {
        public EditGroupChatMessage ToCommand()
            => new(GroupId, MessageId, NewContent);
        
        public EditChatMessageReply ToReplyCommand()
            => new(GroupId, MessageId, NewContent);
    }

    public sealed record DeleteGroupChatMessageRequest(
        [Required] Guid GroupId,
        [Required] Guid MessageId
    )
    {
        public DeleteGroupChatMessage ToCommand()
            => new(GroupId, MessageId);
        
        public DeleteChatMessageReply ToReplyCommand()
            => new(MessageId, GroupId);
    }

    public sealed record GetMessagesByChatGroupRequest(
        [Required] Guid GroupId,
        [Required] int PageSize,
        [Required] string PagingCursor
    )
    {
        public GetMessagesForChatGroup ToCommand()
            => new(GroupId, PageSize, PagingCursor);
    }

    public sealed record GetRepliesByForMessageRequest(
        [Required] Guid MessageId,
        [Required] int PageSize,
        [Required] string PagingCursor
    )
    {
        public GetRepliesByForMessage ToCommand()
            => new(MessageId, PageSize, PagingCursor);
    }
}